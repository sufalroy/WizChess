using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Runtime.InteropServices;

namespace WizChess.Graphics
{
	public class IndexBuffer : System.IDisposable
	{
		public Buffer MyBuffer { get; }

		private IndexBuffer(Buffer buffer)
		{
			MyBuffer = buffer;
		}

		public void Bind()
		{
			Renderer.MyDeviceContext.InputAssembler.SetIndexBuffer(MyBuffer, Format.R32_UInt, 0);
		}

		public void Dispose()
		{
			MyBuffer.Dispose();
		}

		public static IndexBuffer Create<T>(T[] data) where T : struct
		{
			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			DataStream stream = new DataStream(handle.AddrOfPinnedObject(), Utilities.SizeOf(data), true, true);

			BufferDescription desc = new BufferDescription()
			{
				BindFlags = BindFlags.IndexBuffer,
				Usage = ResourceUsage.Default,
				SizeInBytes = Utilities.SizeOf(data),
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None,
				StructureByteStride = 0
			};

			Buffer buffer = new Buffer(Renderer.MyDevice, stream, desc);
			return new IndexBuffer(buffer);
		}
	}
}
