using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct ColorTargetState
{
    public TextureFormat Format;
    public unsafe BlendState? Blend;
    public ColorWriteMask WriteMask;
    public unsafe ColorTargetState(TextureFormat? format = null, BlendState? blend = null, ColorWriteMask? writeMask = null)
    {
        this = default(ColorTargetState);
        if (format.HasValue)
        {
            Format = format.Value;
        }

        if (blend.HasValue)
        {
            Blend = blend.Value;
        }

        if (writeMask.HasValue)
        {
            WriteMask = writeMask.Value;
        }
    }
}