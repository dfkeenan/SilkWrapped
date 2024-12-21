using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public enum ErrorFilter
{
    Validation = 0,
    OutOfMemory = 1,
    Internal = 2,
    Force32 = int.MaxValue
}