using Silk.NET.Core;
using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct RenderPassDepthStencilAttachment
{
    public unsafe TextureViewHandle View;
    public LoadOp DepthLoadOp;
    public StoreOp DepthStoreOp;
    public float DepthClearValue;
    public Bool32 DepthReadOnly;
    public LoadOp StencilLoadOp;
    public StoreOp StencilStoreOp;
    public uint StencilClearValue;
    public Bool32 StencilReadOnly;
    public unsafe RenderPassDepthStencilAttachment(TextureViewHandle? view = null, LoadOp? depthLoadOp = null, StoreOp? depthStoreOp = null, float? depthClearValue = null, Bool32? depthReadOnly = null, LoadOp? stencilLoadOp = null, StoreOp? stencilStoreOp = null, uint? stencilClearValue = null, Bool32? stencilReadOnly = null)
    {
        this = default(RenderPassDepthStencilAttachment);
        if (view.HasValue)
        {
            View = view.Value;
        }

        if (depthLoadOp.HasValue)
        {
            DepthLoadOp = depthLoadOp.Value;
        }

        if (depthStoreOp.HasValue)
        {
            DepthStoreOp = depthStoreOp.Value;
        }

        if (depthClearValue.HasValue)
        {
            DepthClearValue = depthClearValue.Value;
        }

        if (depthReadOnly.HasValue)
        {
            DepthReadOnly = depthReadOnly.Value;
        }

        if (stencilLoadOp.HasValue)
        {
            StencilLoadOp = stencilLoadOp.Value;
        }

        if (stencilStoreOp.HasValue)
        {
            StencilStoreOp = stencilStoreOp.Value;
        }

        if (stencilClearValue.HasValue)
        {
            StencilClearValue = stencilClearValue.Value;
        }

        if (stencilReadOnly.HasValue)
        {
            StencilReadOnly = stencilReadOnly.Value;
        }
    }
}