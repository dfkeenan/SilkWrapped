using Silk.NET.Core;

namespace SilkWrapped.WebGPU;
public partial struct BufferDescriptor
{
    public unsafe string? Label;
    public BufferUsage Usage;
    public ulong Size;
    public Bool32 MappedAtCreation;
    public unsafe BufferDescriptor(string? label = null, BufferUsage? usage = null, ulong? size = null, Bool32? mappedAtCreation = null)
    {
        this = default(BufferDescriptor);
        if (label != null)
        {
            Label = label;
        }

        if (usage.HasValue)
        {
            Usage = usage.Value;
        }

        if (size.HasValue)
        {
            Size = size.Value;
        }

        if (mappedAtCreation.HasValue)
        {
            MappedAtCreation = mappedAtCreation.Value;
        }
    }
}