using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public enum CompilationInfoRequestStatus
{
    Success = 0,
    Error = 1,
    DeviceLost = 2,
    Unknown = 3,
    Force32 = int.MaxValue
}