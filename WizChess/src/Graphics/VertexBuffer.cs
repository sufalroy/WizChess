using SharpDX;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;

namespace WizChess.Graphics
{
	public class VertexBuffer : System.IDisposable
	{
		private readonly Buffer m_Buffer;
		private readonly int m_Stride;

		private VertexBuffer(Buffer buffer, int stride)
		{
			m_Buffer = buffer;
			m_Stride = stride;
		}

		public void Bind(int slot)
		{
			Renderer.MyDeviceContext.InputAssembler.SetVertexBuffers(slot, new VertexBufferBinding(m_Buffer, m_Stride, 0));
		}

		public void Dispose()
		{
			m_Buffer.Dispose();
		}

		public static VertexBuffer Create<T>(T[] data) where T : struct
		{
			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			DataStream stream = new DataStream(handle.AddrOfPinnedObject(), Utilities.SizeOf(data), true, true);

			BufferDescription desc = new BufferDescription()
			{
				BindFlags = BindFlags.VertexBuffer,
				Usage = ResourceUsage.Default,
				SizeInBytes = Utilities.SizeOf(data),
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None,
				StructureByteStride = 0
			};

			Buffer buffer = new Buffer(Renderer.MyDevice, stream, desc);
			return new VertexBuffer(buffer, Utilities.SizeOf<T>());
		}
	}
}
