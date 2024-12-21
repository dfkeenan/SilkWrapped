using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct SamplerBindingLayout
{
    public SamplerBindingType Type;
    public unsafe SamplerBindingLayout(SamplerBindingType? type = null)
    {
        this = default(SamplerBindingLayout);
        if (type.HasValue)
        {
            Type = type.Value;
        }
    }
}