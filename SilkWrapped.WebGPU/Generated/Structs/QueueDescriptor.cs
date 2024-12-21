using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct QueueDescriptor
{
    public unsafe string? Label;
    public unsafe QueueDescriptor(string? label = null)
    {
        this = default(QueueDescriptor);
        if (label != null)
        {
            Label = label;
        }
    }
}