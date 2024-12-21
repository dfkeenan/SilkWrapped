using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct RenderPassColorAttachment
{
    public unsafe TextureViewHandle View;
    public uint DepthSlice;
    public unsafe TextureViewHandle ResolveTarget;
    public LoadOp LoadOp;
    public StoreOp StoreOp;
    public Color ClearValue;
    public unsafe RenderPassColorAttachment(TextureViewHandle? view = null, uint? depthSlice = null, TextureViewHandle? resolveTarget = null, LoadOp? loadOp = null, StoreOp? storeOp = null, Color? clearValue = null)
    {
        this = default(RenderPassColorAttachment);
        if (view.HasValue)
        {
            View = view.Value;
        }

        if (depthSlice.HasValue)
        {
            DepthSlice = depthSlice.Value;
        }

        if (resolveTarget.HasValue)
        {
            ResolveTarget = resolveTarget.Value;
        }

        if (loadOp.HasValue)
        {
            LoadOp = loadOp.Value;
        }

        if (storeOp.HasValue)
        {
            StoreOp = storeOp.Value;
        }

        if (clearValue.HasValue)
        {
            ClearValue = clearValue.Value;
        }
    }
}