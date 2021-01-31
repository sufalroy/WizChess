using System;
using SharpDX.Windows;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using SharpDX.Mathematics.Interop;
using WizChess.Events;
using SharpDX;
using System.Collections.Generic;
using WizChess.Entities.Components;

namespace WizChess.Graphics
{
	public static class Renderer
	{
		public static Device MyDevice => s_Device;
		public static DeviceContext MyDeviceContext { get; private set; }
		public static SwapChain MySwapChain => s_SwapChain;

		private static Device s_Device;
		private static SwapChain s_SwapChain;
		private static Factory s_Factory;
		private static Framebuffer s_ScreenBuffer;

		private static readonly List<DrawCommand> s_DrawCommands = new List<DrawCommand>();

		private static Shader s_BasicShader;

		private static SamplerState s_SamplerState;

		public static void Initialize(RenderForm window)
		{
			SwapChainDescription desc = new SwapChainDescription()
			{
				BufferCount = 1,
				ModeDescription = new ModeDescription(window.ClientSize.Width, window.ClientSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
				IsWindowed = true,
				OutputHandle = window.Handle,
				SampleDescription = new SampleDescription(1, 0),
				SwapEffect = SwapEffect.Discard,
				Usage = Usage.RenderTargetOutput
			};

			Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, desc, out s_Device, out s_SwapChain);
			MyDeviceContext = MyDevice.ImmediateContext;

			s_Factory = MySwapChain.GetParent<Factory>();
			s_Factory.MakeWindowAssociation(window.Handle, WindowAssociationFlags.IgnoreAll);

			MyDeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

			s_BasicShader = ShaderLibrary.Load("Basic", "res/Shaders/Basic.hlsl");

			s_ScreenBuffer = new Framebuffer(window.ClientSize.Width, window.ClientSize.Height);
			s_ScreenBuffer.Bind();

			SamplerStateDescription samplerDesc = new SamplerStateDescription()
			{
				Filter = Filter.MinMagMipLinear,
				AddressU = TextureAddressMode.Wrap,
				AddressV = TextureAddressMode.Wrap,
				AddressW = TextureAddressMode.Wrap,
				MipLodBias = 0.0F,
				ComparisonFunction = Comparison.Always,
				MinimumLod = 0,
				MaximumLod = float.MaxValue
			};

			s_SamplerState = new SamplerState(MyDevice, samplerDesc);
			MyDeviceContext.PixelShader.SetSampler(0, s_SamplerState);

			EventHooks.RegisterHook<ResizeEventArgs>(OnResize);
		}

		private static void OnResize(object sender, EventArgs args)
		{
			ResizeEventArgs e = (ResizeEventArgs)args;

			s_ScreenBuffer.Dispose();
			MySwapChain.ResizeBuffers(MySwapChain.Description.BufferCount, e.Width, e.Height, Format.Unknown, SwapChainFlags.None);
			s_ScreenBuffer.Resize(e.Width, e.Height, true);

			MyDeviceContext.Rasterizer.SetViewport(new Viewport(0, 0, e.Width, e.Height, 0.0F, 1.0F));
			s_ScreenBuffer.Bind();
		}

		public static void Clear(RawColor4 color)
		{
			s_ScreenBuffer.Clear(color);
		}

		public static void BeginDraw(Camera camera)
		{
			s_BasicShader.Bind();

			Matrix viewProj = camera.ViewProjectionMatrix;
			viewProj.Transpose();
			s_BasicShader.Set("CameraData", ref viewProj);
		}

		public static void Submit(Mesh mesh, TransformComponent transform)
		{
			s_DrawCommands.Add(new DrawCommand() { MyMesh = mesh, MyTransform = transform });
		}

		public static void EndDraw()
		{
			foreach (DrawCommand cmd in s_DrawCommands)
			{
				Matrix matrix = cmd.MyTransform.TransformMatrix;
				matrix.Transpose();
				s_BasicShader.Set("ObjectData", ref matrix);
				cmd.MyMesh.Draw();
			}

			MySwapChain.Present(1, PresentFlags.None);
			s_DrawCommands.Clear();
		}

		public static void Close()
		{
			ShaderLibrary.Close();

			s_ScreenBuffer.Dispose();
			MyDeviceContext.ClearState();
			MyDeviceContext.Flush();
			s_Device.Dispose();
			MyDeviceContext.Dispose();
			MySwapChain.Dispose();
			s_Factory.Dispose();
		}

		private struct DrawCommand
		{
			public Mesh MyMesh;
			public TransformComponent MyTransform;
		}
	}
}
