namespace SilkWrapped.WebGPU;
public partial struct BlendComponent
{
    public BlendOperation Operation;
    public BlendFactor SrcFactor;
    public BlendFactor DstFactor;
    public BlendComponent(BlendOperation? operation = null, BlendFactor? srcFactor = null, BlendFactor? dstFactor = null)
    {
        this = default(BlendComponent);
        if (operation.HasValue)
        {
            Operation = operation.Value;
        }

        if (srcFactor.HasValue)
        {
            SrcFactor = srcFactor.Value;
        }

        if (dstFactor.HasValue)
        {
            DstFactor = dstFactor.Value;
        }
    }
}