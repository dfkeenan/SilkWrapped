namespace SilkWrapped.WebGPU;
public partial struct TextureDescriptor
{
    public unsafe string? Label;
    public TextureUsage Usage;
    public TextureDimension Dimension;
    public Extent3D Size;
    public TextureFormat Format;
    public uint MipLevelCount;
    public uint SampleCount;
    public unsafe TextureFormat[]? ViewFormats;
    public unsafe TextureDescriptor(string? label = null, TextureUsage? usage = null, TextureDimension? dimension = null, Extent3D? size = null, TextureFormat? format = null, uint? mipLevelCount = null, uint? sampleCount = null, TextureFormat[]? viewFormats = null)
    {
        this = default(TextureDescriptor);
        if (label != null)
        {
            Label = label;
        }

        if (usage.HasValue)
        {
            Usage = usage.Value;
        }

        if (dimension.HasValue)
        {
            Dimension = dimension.Value;
        }

        if (size.HasValue)
        {
            Size = size.Value;
        }

        if (format.HasValue)
        {
            Format = format.Value;
        }

        if (mipLevelCount.HasValue)
        {
            MipLevelCount = mipLevelCount.Value;
        }

        if (sampleCount.HasValue)
        {
            SampleCount = sampleCount.Value;
        }

        if (viewFormats != null)
        {
            ViewFormats = viewFormats;
        }
    }
}