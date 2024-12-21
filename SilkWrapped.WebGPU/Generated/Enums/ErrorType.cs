using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public enum ErrorType
{
    NoError = 0,
    Validation = 1,
    OutOfMemory = 2,
    Internal = 3,
    Unknown = 4,
    DeviceLost = 5,
    Force32 = int.MaxValue
}