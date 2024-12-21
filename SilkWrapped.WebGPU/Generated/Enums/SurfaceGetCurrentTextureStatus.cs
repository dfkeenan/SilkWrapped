using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public enum SurfaceGetCurrentTextureStatus
{
    Success = 0,
    Timeout = 1,
    Outdated = 2,
    Lost = 3,
    OutOfMemory = 4,
    DeviceLost = 5,
    Force32 = int.MaxValue
}