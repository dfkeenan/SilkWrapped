using Silk.NET.Core;
using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct TextureBindingLayout
{
    public TextureSampleType SampleType;
    public TextureViewDimension ViewDimension;
    public Bool32 Multisampled;
    public unsafe TextureBindingLayout(TextureSampleType? sampleType = null, TextureViewDimension? viewDimension = null, Bool32? multisampled = null)
    {
        this = default(TextureBindingLayout);
        if (sampleType.HasValue)
        {
            SampleType = sampleType.Value;
        }

        if (viewDimension.HasValue)
        {
            ViewDimension = viewDimension.Value;
        }

        if (multisampled.HasValue)
        {
            Multisampled = multisampled.Value;
        }
    }
}