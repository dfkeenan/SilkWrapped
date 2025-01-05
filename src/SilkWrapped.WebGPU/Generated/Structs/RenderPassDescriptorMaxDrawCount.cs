namespace SilkWrapped.WebGPU;
public partial struct RenderPassDescriptorMaxDrawCount
{
    public ulong MaxDrawCount;
    public RenderPassDescriptorMaxDrawCount(ulong? maxDrawCount = null)
    {
        this = default(RenderPassDescriptorMaxDrawCount);
        if (maxDrawCount.HasValue)
        {
            MaxDrawCount = maxDrawCount.Value;
        }
    }
}