using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public enum MapMode
{
    None = 0,
    Read = 1,
    Write = 2,
    Force32 = int.MaxValue
}