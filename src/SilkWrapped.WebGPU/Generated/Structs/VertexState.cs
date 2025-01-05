namespace SilkWrapped.WebGPU;
public partial struct VertexState
{
    public unsafe ShaderModuleHandle Module;
    public unsafe string? EntryPoint;
    public unsafe ConstantEntry[]? Constants;
    public unsafe VertexBufferLayout[]? Buffers;
    public unsafe VertexState(ShaderModuleHandle? module = null, string? entryPoint = null, ConstantEntry[]? constants = null, VertexBufferLayout[]? buffers = null)
    {
        this = default(VertexState);
        if (module.HasValue)
        {
            Module = module.Value;
        }

        if (entryPoint != null)
        {
            EntryPoint = entryPoint;
        }

        if (constants != null)
        {
            Constants = constants;
        }

        if (buffers != null)
        {
            Buffers = buffers;
        }
    }
}