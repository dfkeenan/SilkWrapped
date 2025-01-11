namespace SilkWrapped.WebGPU;
public static class CommandEncoderExtensions
{
    public static RenderPassEncoder BeginRenderPass(
        this CommandEncoder commandEncoder,
        TextureView colorAttachmentView,
        Color? clearColor = null)
    {
        var description = RenderPassDescriptor.Empty
                            .WithColorAttachment(
                                colorAttachmentView,
                                clearColor ?? default,
                                clearColor.HasValue ? LoadOp.Clear : LoadOp.Load);

        return commandEncoder.BeginRenderPass(in description);
    }

    public static RenderPassEncoder BeginRenderPass(
       this CommandEncoder commandEncoder,
       TextureView colorAttachmentView,
       TextureView depthStencilView,
       Color? clearColor = null,
       float? depthClearValue = null)
    {
        var description = RenderPassDescriptor.Empty
                            .WithColorAttachment(
                                colorAttachmentView,
                                clearColor ?? default,
                                clearColor.HasValue ? LoadOp.Clear : LoadOp.Load)
                            .WithDepthStencilAttachment(
                                depthStencilView,
                                depthClearValue ?? default,
                                depthClearValue.HasValue ? LoadOp.Clear : LoadOp.Load);

        return commandEncoder.BeginRenderPass(in description);
    }
}
