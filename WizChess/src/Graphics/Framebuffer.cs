using System;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using Resource = SharpDX.Direct3D11.Resource;


namespace WizChess.Graphics
{
	public class Framebuffer : IDisposable
	{
		private static int s_LastBackBuffer;

		public int Width { get; private set; }
		public int Height { get; private set; }

		private RenderTargetView m_RenderTarget;
		private DepthStencilView m_DepthStencil;

		public Framebuffer(int width, int height)
		{
			Width = width;
			Height = height;

			Resize(width, height, true);
		}

		public void Resize(int width, int height, bool forceRecreate = false)
		{
			if (!forceRecreate && (Width == width && Height == height))
				return;

			Width = width;
			Height = height;

			Utilities.Dispose(ref m_RenderTarget);
			Utilities.Dispose(ref m_DepthStencil);

			Texture2D buffer = Resource.FromSwapChain<Texture2D>(Renderer.MySwapChain, s_LastBackBuffer++);
			m_RenderTarget = new RenderTargetView(Renderer.MyDevice, buffer);
			buffer.Dispose();

			Texture2D depthBuffer = new Texture2D(Renderer.MyDevice, new Texture2DDescription()
			{
				Format = Format.D32_Float_S8X24_UInt,
				ArraySize = 1,
				MipLevels = 1,
				Width = width,
				Height = height,
				SampleDescription = new SampleDescription(1, 0),
				Usage = ResourceUsage.Default,
				BindFlags = BindFlags.DepthStencil,
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None
			});

			m_DepthStencil = new DepthStencilView(Renderer.MyDevice, depthBuffer);
			depthBuffer.Dispose();
		}

		public void Bind()
		{
			Renderer.MyDeviceContext.OutputMerger.SetTargets(m_DepthStencil, m_RenderTarget);
		}

		public void Clear(RawColor4 color)
		{
			Renderer.MyDeviceContext.ClearDepthStencilView(m_DepthStencil, DepthStencilClearFlags.Depth, 1.0F, 0);
			Renderer.MyDeviceContext.ClearRenderTargetView(m_RenderTarget, color);
		}

		public void Dispose()
		{
			m_DepthStencil.Dispose();
			m_RenderTarget.Dispose();

			s_LastBackBuffer--;
		}
	}
}
