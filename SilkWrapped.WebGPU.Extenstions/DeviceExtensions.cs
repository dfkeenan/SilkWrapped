using System.Runtime.CompilerServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SilkWrapped.WebGPU;
public static class DeviceExtensions
{
    public static Texture LoadTexture(this Device device, string fileName, TextureFormat textureFormat = TextureFormat.Rgba8Unorm)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(fileName);

        return textureFormat switch
        {
            TextureFormat.Rgba8Unorm => device.LoadTexture<Rgba32>(fileName),
            _ => throw new NotImplementedException($"Texture format '{textureFormat}' is not supported."),
        };
    }

    private static Texture LoadTexture<TPixel>(this Device device, string fileName, TextureFormat textureFormat = TextureFormat.Rgba8Unorm)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(fileName);

        using var image = Image.Load<TPixel>(fileName);

        var descriptor = new TextureDescriptor
        {
            Size = new Extent3D((uint)image.Width, (uint)image.Height, 1),
            Format = textureFormat,
            Usage = TextureUsage.CopyDst | TextureUsage.TextureBinding,
            MipLevelCount = 1,
            SampleCount = 1,
            Dimension = TextureDimension.Dimension2D,
            ViewFormats = [textureFormat],
        };

        var texture = device.CreateTexture(in descriptor);

        using var queue = device.GetQueue();

        var layout = new TextureDataLayout
        {
            BytesPerRow = (uint)(image.Width * Unsafe.SizeOf<TPixel>()),
            RowsPerImage = (uint)image.Height
        };

        var extent = new Extent3D
        {
            Width = (uint)image.Width,
            Height = 1,
            DepthOrArrayLayers = 1
        };

        image.ProcessPixelRows
        (
            x =>
            {
                for (var i = 0; i < x.Height; i++)
                {
                    var imageRow = x.GetRowSpan(i);

                    var imageCopyTexture = new ImageCopyTexture
                    {
                        Texture = texture,
                        Aspect = TextureAspect.All,
                        MipLevel = 0,
                        Origin = new Origin3D(0, (uint)i, 0)
                    };

                    queue.WriteTexture<TPixel>(in imageCopyTexture, imageRow, in layout, in extent);
                }
            }
        );

        return texture;
    }
}
