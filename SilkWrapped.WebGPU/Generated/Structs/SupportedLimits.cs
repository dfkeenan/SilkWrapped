namespace SilkWrapped.WebGPU;
public partial struct SupportedLimits
{
    public Limits Limits;
    public unsafe SupportedLimits(Limits? limits = null)
    {
        this = default(SupportedLimits);
        if (limits.HasValue)
        {
            Limits = limits.Value;
        }
    }
}