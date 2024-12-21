using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct ComputePassDescriptor
{
    public unsafe string? Label;
    public unsafe ComputePassTimestampWrites? TimestampWrites;
    public unsafe ComputePassDescriptor(string? label = null, ComputePassTimestampWrites? timestampWrites = null)
    {
        this = default(ComputePassDescriptor);
        if (label != null)
        {
            Label = label;
        }

        if (timestampWrites.HasValue)
        {
            TimestampWrites = timestampWrites.Value;
        }
    }
}