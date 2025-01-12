namespace SilkWrapped.WebGPU;
public static class BindGroupExtensions
{
    public static void SetBindGroup<TBindGroup>(this RenderPassEncoder encoder, TBindGroup bindGroup, params ReadOnlySpan<uint> dynamicOffsets)
        where TBindGroup : class, IBindGroup<TBindGroup>
    {
        bindGroup.ApplyChanges();
        encoder.SetBindGroup(TBindGroup.BindGroupIndex, (BindGroupHandle)bindGroup, dynamicOffsets);
    }

    public static void SetBindGroup<TBindGroup>(this RenderBundleEncoder encoder, TBindGroup bindGroup, params ReadOnlySpan<uint> dynamicOffsets)
        where TBindGroup : class, IBindGroup<TBindGroup>
    {
        bindGroup.ApplyChanges();
        encoder.SetBindGroup(TBindGroup.BindGroupIndex, (BindGroupHandle)bindGroup, dynamicOffsets);
    }
}
