namespace SilkWrapped.WebGPU;
public enum QueueWorkDoneStatus
{
    Success = 0,
    Error = 1,
    Unknown = 2,
    DeviceLost = 3,
    Force32 = int.MaxValue
}