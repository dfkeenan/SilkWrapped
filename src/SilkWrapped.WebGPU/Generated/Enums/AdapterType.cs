namespace SilkWrapped.WebGPU;
public enum AdapterType
{
    DiscreteGpu = 0,
    IntegratedGpu = 1,
    Cpu = 2,
    Unknown = 3,
    Force32 = int.MaxValue
}