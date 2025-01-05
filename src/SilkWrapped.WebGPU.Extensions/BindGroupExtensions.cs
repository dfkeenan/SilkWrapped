namespace SilkWrapped.WebGPU;
public static class BindGroupExtensions
{
    public static void SetBindGroup<TBindGroup>(this RenderPassEncoder encoder, uint groupIndex, TBindGroup bindGroup, params ReadOnlySpan<uint> dynamicOffsets)
        where TBindGroup : class, IBindGroup<TBindGroup>
    {
        bindGroup.ApplyChanges();
        encoder.SetBindGroup(groupIndex, (BindGroupHandle)bindGroup, dynamicOffsets);
    }

    public static void SetBindGroup<TBindGroup>(this RenderBundleEncoder encoder, uint groupIndex, TBindGroup bindGroup, params ReadOnlySpan<uint> dynamicOffsets)
        where TBindGroup : class, IBindGroup<TBindGroup>
    {
        bindGroup.ApplyChanges();
        encoder.SetBindGroup(groupIndex, (BindGroupHandle)bindGroup, dynamicOffsets);
    }
}
