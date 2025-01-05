namespace SilkWrapped.WebGPU;
public partial struct BlendState
{
    public BlendComponent Color;
    public BlendComponent Alpha;
    public BlendState(BlendComponent? color = null, BlendComponent? alpha = null)
    {
        this = default(BlendState);
        if (color.HasValue)
        {
            Color = color.Value;
        }

        if (alpha.HasValue)
        {
            Alpha = alpha.Value;
        }
    }
}