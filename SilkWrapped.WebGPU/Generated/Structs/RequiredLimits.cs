using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct RequiredLimits
{
    public Limits Limits;
    public unsafe RequiredLimits(Limits? limits = null)
    {
        this = default(RequiredLimits);
        if (limits.HasValue)
        {
            Limits = limits.Value;
        }
    }
}