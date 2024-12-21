using Silk.NET.Core;
using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct PrimitiveDepthClipControl
{
    public Bool32 UnclippedDepth;
    public PrimitiveDepthClipControl(Bool32? unclippedDepth = null)
    {
        this = default(PrimitiveDepthClipControl);
        if (unclippedDepth.HasValue)
        {
            UnclippedDepth = unclippedDepth.Value;
        }
    }
}