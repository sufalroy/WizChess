using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Assimp;
using Assimp.Configs;
using SharpDX;

namespace WizChess.Graphics
{
	[StructLayout(LayoutKind.Sequential)]
	public struct VertexData
	{
		public readonly Vector3 Position;
		public readonly Vector3 Normal;
		public readonly Vector2 UV;

		public VertexData(Vector3D position, Vector3D normal, Vector3D uv)
		{
			Position = new Vector3(position.X, position.Y, position.Z);
			Normal = new Vector3(normal.X, normal.Y, normal.Z);
			UV = new Vector2(uv.X, 1 - uv.Y);
		}
	}

	public struct MeshPart
	{
		public int IndexOffset { get; }
		public int IndexCount { get; }

		public MeshPart(int indexOffset, int indexCount)
		{
			IndexOffset = indexOffset;
			IndexCount = indexCount;
		}
	}

	public class Mesh : IDisposable
	{
		public readonly string Filepath;

		private readonly List<MeshPart> m_MeshParts;

		private VertexBuffer m_VertexBuffer;
		private IndexBuffer m_IndexBuffer;

		public Mesh(string filepath)
		{
			Filepath = filepath;
			m_MeshParts = new List<MeshPart>();
			Load();
		}

		private void Load()
		{
			if (!File.Exists(Filepath))
			{
				Console.WriteLine($"File '{Filepath}' doesn't exist!");
				return;
			}

			AssimpContext context = new AssimpContext();
			context.SetConfig(new NormalSmoothingAngleConfig(66.0F));

			Scene scene = context.ImportFile(Filepath, PostProcessSteps.Triangulate | PostProcessSteps.GenerateUVCoords | PostProcessSteps.GenerateNormals);
			if (scene == null)
			{
				Console.WriteLine($"Mesh '{Filepath}' isn't valid!");
				return;
			}

			GetVertexCounts(scene, out int vertexCount, out int indexCount);

			if (vertexCount == 0 || indexCount == 0)
			{
				Console.WriteLine("Invalid Mesh!");
				return;
			}

			VertexData[] vertices = new VertexData[vertexCount];
			uint[] indices = new uint[indexCount];

			int vertexIndex = 0;
			int indexIndex = 0;
			int vertexOffset = 0;
			int indexOffset = 0;

			foreach (Assimp.Mesh mesh in scene.Meshes)
			{
				List<Vector3D> vertexPositions = mesh.Vertices;
				List<Vector3D> normals = mesh.HasNormals ? mesh.Normals : null;
				List<Vector3D> uvs = mesh.HasTextureCoords(0) ? mesh.TextureCoordinateChannels[0] : null;

				for (int i = 0; i < vertexPositions.Count; i++)
				{
					Vector3D position = vertexPositions[i];
					Vector3D normal = normals?[i] ?? new Vector3D(0);
					Vector3D uv = uvs?[i] ?? new Vector3D(0);

					vertices[vertexIndex++] = new VertexData(position, normal, uv);
				}

				List<Face> faces = mesh.Faces;
				foreach (Face face in faces)
				{
					if (face.IndexCount != 3)
					{
						indices[indexIndex++] = 0;
						indices[indexIndex++] = 0;
						indices[indexIndex++] = 0;
						continue;
					}

					indices[indexIndex++] = (uint)(face.Indices[0] + vertexOffset);
					indices[indexIndex++] = (uint)(face.Indices[1] + vertexOffset);
					indices[indexIndex++] = (uint)(face.Indices[2] + vertexOffset);
				}

				int indexCountForMesh = faces.Count * 3;
				m_MeshParts.Add(new MeshPart(indexOffset, indexCountForMesh));

				vertexOffset += vertexPositions.Count;
				indexOffset += indexCountForMesh;
			}

			m_VertexBuffer = VertexBuffer.Create(vertices);
			m_IndexBuffer = IndexBuffer.Create(indices);
		}

		public void Draw()
		{
			m_VertexBuffer.Bind(0);
			m_IndexBuffer.Bind();

			foreach (MeshPart part in m_MeshParts)
				Renderer.MyDeviceContext.DrawIndexed(part.IndexCount, part.IndexOffset, 0);
		}

		private void GetVertexCounts(Scene scene, out int vertexCount, out int indexCount)
		{
			vertexCount = 0;
			indexCount = 0;

			foreach (Assimp.Mesh mesh in scene.Meshes)
			{
				vertexCount += mesh.VertexCount;
				indexCount += 3 * mesh.FaceCount;
			}
		}

		public void Dispose()
		{
			m_VertexBuffer.Dispose();
			m_IndexBuffer.Dispose();
			m_MeshParts.Clear();
		}
	}
}
