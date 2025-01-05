namespace SilkWrapped.WebGPU;
public partial struct ProgrammableStageDescriptor
{
    public unsafe ShaderModuleHandle Module;
    public unsafe string? EntryPoint;
    public unsafe ConstantEntry[]? Constants;
    public unsafe ProgrammableStageDescriptor(ShaderModuleHandle? module = null, string? entryPoint = null, ConstantEntry[]? constants = null)
    {
        this = default(ProgrammableStageDescriptor);
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
    }
}