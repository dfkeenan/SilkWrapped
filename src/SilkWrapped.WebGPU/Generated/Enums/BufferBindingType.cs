namespace SilkWrapped.WebGPU;
public enum BufferBindingType
{
    Undefined = 0,
    Uniform = 1,
    Storage = 2,
    ReadOnlyStorage = 3,
    Force32 = int.MaxValue
}