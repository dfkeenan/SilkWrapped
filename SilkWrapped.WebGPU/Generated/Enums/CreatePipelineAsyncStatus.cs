using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public enum CreatePipelineAsyncStatus
{
    Success = 0,
    ValidationError = 1,
    InternalError = 2,
    DeviceLost = 3,
    DeviceDestroyed = 4,
    Unknown = 5,
    Force32 = int.MaxValue
}