namespace SilkWrapped.WebGPU;
public partial struct FragmentState
{
    public unsafe ShaderModuleHandle Module;
    public unsafe string? EntryPoint;
    public unsafe ConstantEntry[]? Constants;
    public unsafe ColorTargetState[]? Targets;
    public unsafe FragmentState(ShaderModuleHandle? module = null, string? entryPoint = null, ConstantEntry[]? constants = null, ColorTargetState[]? targets = null)
    {
        this = default(FragmentState);
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

        if (targets != null)
        {
            Targets = targets;
        }
    }
}