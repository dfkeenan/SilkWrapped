using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public enum AddressMode
{
    Repeat = 0,
    MirrorRepeat = 1,
    ClampToEdge = 2,
    Force32 = int.MaxValue
}