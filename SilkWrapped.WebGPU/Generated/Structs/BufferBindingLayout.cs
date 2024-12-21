using Silk.NET.Core;

namespace SilkWrapped.WebGPU;
public partial struct BufferBindingLayout
{
    public BufferBindingType Type;
    public Bool32 HasDynamicOffset;
    public ulong MinBindingSize;
    public unsafe BufferBindingLayout(BufferBindingType? type = null, Bool32? hasDynamicOffset = null, ulong? minBindingSize = null)
    {
        this = default(BufferBindingLayout);
        if (type.HasValue)
        {
            Type = type.Value;
        }

        if (hasDynamicOffset.HasValue)
        {
            HasDynamicOffset = hasDynamicOffset.Value;
        }

        if (minBindingSize.HasValue)
        {
            MinBindingSize = minBindingSize.Value;
        }
    }
}