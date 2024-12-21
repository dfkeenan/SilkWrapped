using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public enum MipmapFilterMode
{
    Nearest = 0,
    Linear = 1,
    Force32 = int.MaxValue
}