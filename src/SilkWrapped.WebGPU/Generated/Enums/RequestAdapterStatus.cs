namespace SilkWrapped.WebGPU;
public enum RequestAdapterStatus
{
    Success = 0,
    Unavailable = 1,
    Error = 2,
    Unknown = 3,
    Force32 = int.MaxValue
}