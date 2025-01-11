using System.Runtime.CompilerServices;

namespace SilkWrapped.WebGPU;
public static class RenderPassDescriptorExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RenderPassDescriptor WithColorAttachment(
        this in RenderPassDescriptor descriptor,
        TextureView view,
        Color clearValue,
        LoadOp loadOp = LoadOp.Clear,
        StoreOp storeOp = StoreOp.Store,
        uint depthSlice = 0,
        TextureViewHandle? resolveTarget = null)
    {
        return descriptor with
        {
            ColorAttachments =
            [
                new RenderPassColorAttachment
                {
                    View = view,
                    ClearValue = clearValue,
                    LoadOp = loadOp,
                    StoreOp = storeOp,
                    DepthSlice = depthSlice,
                    ResolveTarget = resolveTarget ?? default,
                }
            ]
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RenderPassDescriptor WithDepthStencilAttachment(
        this in RenderPassDescriptor descriptor,
        TextureView view,
        float clearValue = 1.0f,
        LoadOp loadOp = LoadOp.Clear,
        StoreOp storeOp = StoreOp.Store)
    {
        return descriptor with
        {
            DepthStencilAttachment = new()
            {
                View = view,
                DepthClearValue = clearValue,
                DepthLoadOp = loadOp,
                DepthStoreOp = storeOp,
            }
        };
    }
}
