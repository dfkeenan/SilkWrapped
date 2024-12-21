using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct RenderBundleDescriptor
{
    public unsafe string? Label;
    public unsafe RenderBundleDescriptor(string? label = null)
    {
        this = default(RenderBundleDescriptor);
        if (label != null)
        {
            Label = label;
        }
    }
}