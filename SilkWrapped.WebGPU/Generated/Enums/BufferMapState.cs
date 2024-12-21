using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public enum BufferMapState
{
    Unmapped = 0,
    Pending = 1,
    Mapped = 2,
    Force32 = int.MaxValue
}