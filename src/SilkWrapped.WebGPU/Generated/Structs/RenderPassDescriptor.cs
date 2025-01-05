namespace SilkWrapped.WebGPU;
public partial struct RenderPassDescriptor
{
    public unsafe string? Label;
    public unsafe RenderPassColorAttachment[]? ColorAttachments;
    public unsafe RenderPassDepthStencilAttachment? DepthStencilAttachment;
    public unsafe QuerySetHandle OcclusionQuerySet;
    public unsafe RenderPassTimestampWrites? TimestampWrites;
    public unsafe RenderPassDescriptor(string? label = null, RenderPassColorAttachment[]? colorAttachments = null, RenderPassDepthStencilAttachment? depthStencilAttachment = null, QuerySetHandle? occlusionQuerySet = null, RenderPassTimestampWrites? timestampWrites = null)
    {
        this = default(RenderPassDescriptor);
        if (label != null)
        {
            Label = label;
        }

        if (colorAttachments != null)
        {
            ColorAttachments = colorAttachments;
        }

        if (depthStencilAttachment.HasValue)
        {
            DepthStencilAttachment = depthStencilAttachment.Value;
        }

        if (occlusionQuerySet.HasValue)
        {
            OcclusionQuerySet = occlusionQuerySet.Value;
        }

        if (timestampWrites.HasValue)
        {
            TimestampWrites = timestampWrites.Value;
        }
    }
}