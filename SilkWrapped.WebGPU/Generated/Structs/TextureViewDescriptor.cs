using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct TextureViewDescriptor
{
    public unsafe string? Label;
    public TextureFormat Format;
    public TextureViewDimension Dimension;
    public uint BaseMipLevel;
    public uint MipLevelCount;
    public uint BaseArrayLayer;
    public uint ArrayLayerCount;
    public TextureAspect Aspect;
    public unsafe TextureViewDescriptor(string? label = null, TextureFormat? format = null, TextureViewDimension? dimension = null, uint? baseMipLevel = null, uint? mipLevelCount = null, uint? baseArrayLayer = null, uint? arrayLayerCount = null, TextureAspect? aspect = null)
    {
        this = default(TextureViewDescriptor);
        if (label != null)
        {
            Label = label;
        }

        if (format.HasValue)
        {
            Format = format.Value;
        }

        if (dimension.HasValue)
        {
            Dimension = dimension.Value;
        }

        if (baseMipLevel.HasValue)
        {
            BaseMipLevel = baseMipLevel.Value;
        }

        if (mipLevelCount.HasValue)
        {
            MipLevelCount = mipLevelCount.Value;
        }

        if (baseArrayLayer.HasValue)
        {
            BaseArrayLayer = baseArrayLayer.Value;
        }

        if (arrayLayerCount.HasValue)
        {
            ArrayLayerCount = arrayLayerCount.Value;
        }

        if (aspect.HasValue)
        {
            Aspect = aspect.Value;
        }
    }
}