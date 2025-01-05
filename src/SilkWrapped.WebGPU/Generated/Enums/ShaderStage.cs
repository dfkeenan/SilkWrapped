namespace SilkWrapped.WebGPU;
[Flags]
public enum ShaderStage
{
    None = 0,
    Vertex = 1,
    Fragment = 2,
    Compute = 4,
    Force32 = int.MaxValue
}