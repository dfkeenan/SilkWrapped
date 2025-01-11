using System.Runtime.CompilerServices;
using Silk.NET.Maths;
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

    private static Texture LoadTexture<TPixel>(
        this Device device,
        string fileName,
        TextureFormat textureFormat = TextureFormat.Rgba8Unorm,
        uint sampleCount = 1,
        uint mipLevelCount = 1)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(fileName);

        using var image = Image.Load<TPixel>(fileName);

        var texture = device.CreateTexture(
                                (uint)image.Width,
                                (uint)image.Height,
                                textureFormat,
                                TextureUsage.CopyDst | TextureUsage.TextureBinding,
                                sampleCount,
                                mipLevelCount);

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

    public static Texture CreateTexture(
        this Device device,
        Vector2D<int> size,
        TextureFormat format,
        TextureUsage usage,
        uint sampleCount = 1,
        uint mipLevelCount = 1,
        params ReadOnlySpan<TextureFormat> viewFormats)
    {
        var texture = device.CreateTexture(
                                (uint)size.X,
                                (uint)size.Y,
                                format,
                                usage,
                                sampleCount,
                                mipLevelCount,
                                viewFormats);

        return texture;
    }
}
