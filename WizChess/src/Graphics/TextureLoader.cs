using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.WIC;

namespace WizChess.Graphics
{
    public static class TextureLoader
    {
        private static readonly ImagingFactory s_ImagingFactory = new ImagingFactory();

        private static BitmapSource LoadBitmap(string filename)
        {
            BitmapDecoder decoder = new BitmapDecoder(s_ImagingFactory, filename, DecodeOptions.CacheOnDemand);
            BitmapFrameDecode frame = decoder.GetFrame(0);
            FormatConverter converter = new FormatConverter(s_ImagingFactory);
            converter.Initialize(frame, PixelFormat.Format32bppPRGBA, BitmapDitherType.None, null, 0.0, BitmapPaletteType.Custom);

            return converter;
        }

        public static Texture2D LoadFromFile(string filename)
        {
            BitmapSource source = LoadBitmap(filename);

            Texture2DDescription desc = new Texture2DDescription()
            {
                Width = source.Size.Width,
                Height = source.Size.Height,
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.R8G8B8A8_UNorm,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0)
            };

            DataStream stream = new DataStream(desc.Width * desc.Height * 4, true, true);
            source.CopyPixels(desc.Width * 4, stream);
            DataRectangle rect = new DataRectangle(stream.DataPointer, desc.Width * 4);
            return new Texture2D(Renderer.MyDevice, desc, rect);
        }
    }
}
