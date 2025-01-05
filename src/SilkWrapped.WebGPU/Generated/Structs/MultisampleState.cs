using Silk.NET.Core;

namespace SilkWrapped.WebGPU;
public partial struct MultisampleState
{
    public uint Count;
    public uint Mask;
    public Bool32 AlphaToCoverageEnabled;
    public unsafe MultisampleState(uint? count = null, uint? mask = null, Bool32? alphaToCoverageEnabled = null)
    {
        this = default(MultisampleState);
        if (count.HasValue)
        {
            Count = count.Value;
        }

        if (mask.HasValue)
        {
            Mask = mask.Value;
        }

        if (alphaToCoverageEnabled.HasValue)
        {
            AlphaToCoverageEnabled = alphaToCoverageEnabled.Value;
        }
    }
}