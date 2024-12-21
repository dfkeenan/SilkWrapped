using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct SurfaceDescriptor
{
    public unsafe string? Label;
    public unsafe SurfaceDescriptor(string? label = null)
    {
        this = default(SurfaceDescriptor);
        if (label != null)
        {
            Label = label;
        }
    }
}