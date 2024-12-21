using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public enum RequestDeviceStatus
{
    Success = 0,
    Error = 1,
    Unknown = 2,
    Force32 = int.MaxValue
}