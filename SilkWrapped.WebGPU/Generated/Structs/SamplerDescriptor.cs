using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct SamplerDescriptor
{
    public unsafe string? Label;
    public AddressMode AddressModeU;
    public AddressMode AddressModeV;
    public AddressMode AddressModeW;
    public FilterMode MagFilter;
    public FilterMode MinFilter;
    public MipmapFilterMode MipmapFilter;
    public float LodMinClamp;
    public float LodMaxClamp;
    public CompareFunction Compare;
    public ushort MaxAnisotropy;
    public unsafe SamplerDescriptor(string? label = null, AddressMode? addressModeU = null, AddressMode? addressModeV = null, AddressMode? addressModeW = null, FilterMode? magFilter = null, FilterMode? minFilter = null, MipmapFilterMode? mipmapFilter = null, float? lodMinClamp = null, float? lodMaxClamp = null, CompareFunction? compare = null, ushort? maxAnisotropy = null)
    {
        this = default(SamplerDescriptor);
        if (label != null)
        {
            Label = label;
        }

        if (addressModeU.HasValue)
        {
            AddressModeU = addressModeU.Value;
        }

        if (addressModeV.HasValue)
        {
            AddressModeV = addressModeV.Value;
        }

        if (addressModeW.HasValue)
        {
            AddressModeW = addressModeW.Value;
        }

        if (magFilter.HasValue)
        {
            MagFilter = magFilter.Value;
        }

        if (minFilter.HasValue)
        {
            MinFilter = minFilter.Value;
        }

        if (mipmapFilter.HasValue)
        {
            MipmapFilter = mipmapFilter.Value;
        }

        if (lodMinClamp.HasValue)
        {
            LodMinClamp = lodMinClamp.Value;
        }

        if (lodMaxClamp.HasValue)
        {
            LodMaxClamp = lodMaxClamp.Value;
        }

        if (compare.HasValue)
        {
            Compare = compare.Value;
        }

        if (maxAnisotropy.HasValue)
        {
            MaxAnisotropy = maxAnisotropy.Value;
        }
    }
}