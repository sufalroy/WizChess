using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace WizChess.Graphics
{
	public enum ShaderType
	{
		None = -1, Vertex, Pixel
	}

	public static class MyStringExtensions
	{
		public static int? FindFirstNotOf(this string source, string chars, int startIndex = 0)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (chars == null) throw new ArgumentNullException(nameof(chars));
			if (source.Length == 0) return null;
			if (chars.Length == 0) return 0;

			if (startIndex >= source.Length)
				throw new ArgumentOutOfRangeException();

			for (int i = startIndex; i < source.Length; i++)
			{
				if (chars.IndexOf(source[i]) == -1) return i;
			}
			return null;
		}
	}

	public readonly struct UniformBuffer
	{
		public readonly string Name;
		public readonly int Register;
		public readonly Buffer DataBuffer;

		public UniformBuffer(string name, int register, Buffer dataBuffer)
		{
			Name = name;
			Register = register;
			DataBuffer = dataBuffer;
		}
	}

	public class ShaderResource
	{
		public readonly string Name;
		public readonly int Register;
		public ShaderResourceView Resource;

		public ShaderResource(string name, int register)
		{
			Name = name;
			Register = register;
		}
	}

	public class Shader : IDisposable
	{
		public readonly string Filepath;

		private VertexShader m_VertexShader;
		private PixelShader m_PixelShader;
		private InputLayout m_Layout;

		private readonly List<UniformBuffer> m_VSUniformBuffers = new List<UniformBuffer>();
		private readonly List<UniformBuffer> m_PSUniformBuffers = new List<UniformBuffer>();
		private readonly List<ShaderResource> m_PSResources = new List<ShaderResource>();

		private const string EntryPoint = "main";

		public Shader(string filepath)
		{
			Filepath = filepath;
			Load();
		}

		private void Load()
		{
			if (!File.Exists(Filepath))
				throw new FileNotFoundException($"Shader file {Filepath} not found!");

			string source = File.ReadAllText(Filepath);
			Dictionary<ShaderType, string> sources = PreProcess(source);

			CompilationResult vsByteCode = ShaderBytecode.Compile(sources[ShaderType.Vertex], EntryPoint, "vs_5_0");
			m_VertexShader = new VertexShader(Renderer.MyDevice, vsByteCode);
			CompilationResult psByteCode = ShaderBytecode.Compile(sources[ShaderType.Pixel], EntryPoint, "ps_5_0");
			m_PixelShader = new PixelShader(Renderer.MyDevice, psByteCode);

			ParseInputLayout(sources[ShaderType.Vertex], ShaderSignature.GetInputSignature(vsByteCode));
			ParseUniformBuffers(ref sources);
			ParseResources(sources[ShaderType.Pixel]);
			// TODO: Parse Structs (that aren't the Input Layout)

			vsByteCode.Dispose();
			psByteCode.Dispose();
		}

		private static Format FormatFromStringType(string type)
		{
			switch (type)
			{
				case "float2": return Format.R32G32_Float;
				case "float3": return Format.R32G32B32_Float;
				case "float4": return Format.R32G32B32A32_Float;
			}

			return Format.Unknown;
		}

		private void ParseInputLayout(string vertexSource, ShaderSignature signature)
		{
			int mainPosition = vertexSource.IndexOf(EntryPoint, StringComparison.Ordinal);
			int parametersStart = vertexSource.IndexOf("(", mainPosition, StringComparison.Ordinal);
			int parametersEnd = vertexSource.IndexOf(")", parametersStart, StringComparison.Ordinal);

			string parameters = vertexSource.Substring(parametersStart + 1, parametersEnd - parametersStart - 1);
			string inputLayoutStructName = parameters.Split(' ')[0];

			int structPosition = vertexSource.IndexOf(inputLayoutStructName, StringComparison.Ordinal);
			int structBodyStart = vertexSource.IndexOf("{", structPosition, StringComparison.Ordinal);
			int structBodyEnd = vertexSource.IndexOf("}", structBodyStart, StringComparison.Ordinal);

			string inputLayoutBody = vertexSource.Substring(structBodyStart + 1, structBodyEnd - structBodyStart - 1).Trim();

			List<InputElement> elements = new List<InputElement>();
			foreach (string line in inputLayoutBody.Split('\r', '\n'))
			{
				if (string.IsNullOrEmpty(line))
					continue;

				string type = line.Substring(0, line.IndexOf(' ')).TrimStart();

				int semanticStart = line.IndexOf(":", StringComparison.Ordinal);
				string semantic = line.Substring(semanticStart, line.Length - semanticStart).Trim(' ', ';', ':');

				if (int.TryParse(semantic[semantic.Length - 1].ToString(), out int index))
				{
					semantic = semantic.Remove(semantic.Length - 1, 1);
				}

				elements.Add(new InputElement(semantic, index, FormatFromStringType(type), elements.Count == 0 ? 0 : InputElement.AppendAligned, 0));
			}

			m_Layout = new InputLayout(Renderer.MyDevice, signature, elements.ToArray());

			signature.Dispose();
		}

		private static int GetTypeSizeFromString(string type)
		{
			switch (type)
			{
				case "float2": return sizeof(float) * 2;
				case "float3": return sizeof(float) * 3;
				case "float4": return sizeof(float) * 4;
				case "float4x4": return sizeof(float) * 4 * 4;
			}

			return 0;
		}

		private void ParseUniformBuffers(ref Dictionary<ShaderType, string> sources)
		{
			foreach (ShaderType domain in sources.Keys)
			{
				ParseUniformBuffers(domain, sources[domain]);
			}
		}

		private void ParseUniformBuffers(ShaderType shaderDomain, string source)
		{
			int bufferStart = source.IndexOf("cbuffer", StringComparison.Ordinal);

			while (bufferStart != -1)
			{
				int bufferEnd = source.IndexOf("}", bufferStart, StringComparison.Ordinal);
				string bufferSource = source.Substring(bufferStart, bufferEnd - bufferStart + 1);

				int eol = bufferSource.IndexOf("\r\n", StringComparison.Ordinal);
				string[] nameParts = bufferSource.Substring(0, eol).Trim().Split(' ', ':');
				nameParts = nameParts.Where(x => !string.IsNullOrEmpty(x)).ToArray();

				string bufferName = "";
				int registerNumber = -1;

				if (nameParts.Length <= 2)
				{
					// No register specified, use the next available one
				}
				else
				{
					bufferName = nameParts[1];
					registerNumber = GetAndCheckRegister(nameParts[2], 'b');
				}

				int bodyStart = bufferSource.IndexOf("{", StringComparison.Ordinal);
				int bodyEnd = bufferSource.IndexOf("}", bodyStart, StringComparison.Ordinal);
				string[] memberList = bufferSource.Substring(bodyStart + 1, bodyEnd - bodyStart - 1).Split('\r', '\n');
				memberList = memberList.Where(x => !string.IsNullOrEmpty(x)).ToArray();

				int bufferSize = 0;

				foreach (string member in memberList)
				{
					string type = member.TrimStart().TrimEnd().Substring(0, member.IndexOf(' ')).TrimStart().TrimEnd();
					bufferSize += GetTypeSizeFromString(type);
				}

				Buffer buffer = new Buffer(Renderer.MyDevice, bufferSize, ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

				if (shaderDomain == ShaderType.Vertex)
					m_VSUniformBuffers.Add(new UniformBuffer(bufferName, registerNumber, buffer));
				else
					m_PSUniformBuffers.Add(new UniformBuffer(bufferName, registerNumber, buffer));

				bufferStart = source.IndexOf("cbuffer", bufferEnd, StringComparison.Ordinal);
			}
		}

		private int GetAndCheckRegister(string registerStr, char expectedRegister)
		{
			int registerStart = registerStr.IndexOf("(", StringComparison.Ordinal);
			string registerValue = registerStr.Substring(registerStart + 1, registerStr.IndexOf(")", StringComparison.Ordinal) - registerStart - 1);

			if (registerValue[0] != expectedRegister)
			{
				Console.WriteLine("Invalid register!");
				return -1;
			}

			return int.Parse(registerValue[1].ToString());
		}

		private void ParseResources(string source)
		{
			int pos = 0;
			while (pos != -1)
				pos = ParseResource("Texture2D", source, pos);
		}

		private int ParseResource(string resourceToken, string source, int position)
		{
			if (position == 0)
				position = source.IndexOf(resourceToken, StringComparison.Ordinal);

			int end = source.IndexOf(";", position, StringComparison.Ordinal);
			string[] tokens = source.Substring(position, end - position).Split();

			string name = tokens[1];
			string registerStr = tokens[tokens.Length - 1];

			int register = GetAndCheckRegister(registerStr, 't');

			if (register != -1)
				m_PSResources.Add(new ShaderResource(name, register));

			return source.IndexOf(resourceToken, end + 1, StringComparison.Ordinal);
		}

		private static ShaderType ShaderTypeFromString(string type)
		{
			switch (type)
			{
				case "vertex":
					return ShaderType.Vertex;
				case "pixel":
					return ShaderType.Pixel;
			}

			return ShaderType.None;
		}

		private Dictionary<ShaderType, string> PreProcess(string source)
		{
			Dictionary<ShaderType, string> result = new Dictionary<ShaderType, string>();

			const string typeToken = "#type";
			int typeTokenLength = typeToken.Length;
			int position = source.IndexOf(typeToken, StringComparison.Ordinal);

			while (position != -1)
			{
				int eol = source.IndexOf("\r\n", position, StringComparison.Ordinal);
				int begin = position + typeTokenLength + 1;
				string type = source.Substring(begin, eol - begin);
				int? nextLinePos = source.FindFirstNotOf("\r\n", eol);

				if (nextLinePos == null)
					break;

				position = source.IndexOf(typeToken, (int)nextLinePos, StringComparison.Ordinal);
				ShaderType shaderType = ShaderTypeFromString(type);

				if (position != -1)
					result.Add(shaderType, source.Substring((int)nextLinePos, position - (nextLinePos == -1 ? source.Length - 1 : (int)nextLinePos)));
				else
					result.Add(shaderType, source.Substring((int)nextLinePos, (source.Length - (int)nextLinePos - 1)));
			}

			return result;
		}

		public void Bind()
		{
			Renderer.MyDeviceContext.InputAssembler.InputLayout = m_Layout;

			foreach (UniformBuffer buffer in m_VSUniformBuffers)
			{
				Renderer.MyDeviceContext.VertexShader.SetConstantBuffer(buffer.Register, buffer.DataBuffer);
			}

			foreach (UniformBuffer buffer in m_PSUniformBuffers)
			{
				Renderer.MyDeviceContext.PixelShader.SetConstantBuffer(buffer.Register, buffer.DataBuffer);
			}

			foreach (ShaderResource resource in m_PSResources)
			{
				Renderer.MyDeviceContext.PixelShader.SetShaderResource(resource.Register, resource.Resource);
			}

			Renderer.MyDeviceContext.VertexShader.Set(m_VertexShader);
			Renderer.MyDeviceContext.PixelShader.Set(m_PixelShader);
		}

		public void Set<T>(string bufferName, ref T value) where T : struct
		{
			UniformBuffer buffer = FindBuffer(bufferName);
			Renderer.MyDeviceContext.UpdateSubresource(ref value, buffer.DataBuffer);
		}

		public void Set(string textureName, Texture2D texture)
		{
			foreach (ShaderResource resource in m_PSResources)
			{
				if (resource.Name != textureName)
					continue;

				resource.Resource = new ShaderResourceView(Renderer.MyDevice, texture);
			}
		}

		private UniformBuffer FindBuffer(string name)
		{
			foreach (UniformBuffer buffer in m_VSUniformBuffers)
			{
				if (buffer.Name == name)
					return buffer;
			}

			foreach (UniformBuffer buffer in m_PSUniformBuffers)
			{
				if (buffer.Name == name)
					return buffer;
			}

			throw new ArgumentException();
		}

		public void Dispose()
		{
			m_VSUniformBuffers.ForEach(buffer => buffer.DataBuffer.Dispose());
			m_PSUniformBuffers.ForEach(buffer => buffer.DataBuffer.Dispose());

			m_Layout.Dispose();
			m_VertexShader.Dispose();
			m_PixelShader.Dispose();
		}
	}
}
