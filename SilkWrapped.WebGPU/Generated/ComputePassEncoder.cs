namespace SilkWrapped.WebGPU;
public unsafe readonly struct ComputePassEncoderHandle
{
    private readonly Silk.NET.WebGPU.ComputePassEncoder* nativeHandle;
    private ComputePassEncoderHandle(Silk.NET.WebGPU.ComputePassEncoder* nativeHandle)
    {
        this.nativeHandle = nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.ComputePassEncoder*(ComputePassEncoderHandle handle) => handle.nativeHandle;
    public static implicit operator ComputePassEncoderHandle(Silk.NET.WebGPU.ComputePassEncoder* handle) => new ComputePassEncoderHandle(handle);
}

public unsafe partial class ComputePassEncoder : System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public ComputePassEncoderHandle Handle { get; private set; }

    public ComputePassEncoder(Silk.NET.WebGPU.WebGPU webGPU, ComputePassEncoderHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator ComputePassEncoderHandle(ComputePassEncoder obj) => obj.Handle;
    public unsafe void DispatchWorkgroups(uint workgroupCountX, uint workgroupCountY, uint workgroupCountZ)
    {
        WebGPU.ComputePassEncoderDispatchWorkgroups(Handle, workgroupCountX, workgroupCountY, workgroupCountZ);
    }

    public unsafe void DispatchWorkgroupsIndirect(BufferHandle indirectBuffer, ulong indirectOffset)
    {
        WebGPU.ComputePassEncoderDispatchWorkgroupsIndirect(Handle, indirectBuffer, indirectOffset);
    }

    public unsafe void End()
    {
        WebGPU.ComputePassEncoderEnd(Handle);
    }

    public unsafe void InsertDebugMarker(string markerLabel)
    {
        using var m = new MarshalHelper();
        WebGPU.ComputePassEncoderInsertDebugMarker(Handle, m.RentUtf8Ptr(markerLabel));
    }

    public unsafe void PopDebugGroup()
    {
        WebGPU.ComputePassEncoderPopDebugGroup(Handle);
    }

    public unsafe void PushDebugGroup(string groupLabel)
    {
        using var m = new MarshalHelper();
        WebGPU.ComputePassEncoderPushDebugGroup(Handle, m.RentUtf8Ptr(groupLabel));
    }

    public unsafe void SetBindGroup(uint groupIndex, BindGroupHandle group, params ReadOnlySpan<uint> dynamicOffsets)
    {
        fixed (UInt32* dynamicOffsetsPtr = dynamicOffsets)
        {
            WebGPU.ComputePassEncoderSetBindGroup(Handle, groupIndex, group, (nuint)dynamicOffsets.Length, dynamicOffsetsPtr);
        }
    }

    public unsafe void SetLabel(string label)
    {
        using var m = new MarshalHelper();
        WebGPU.ComputePassEncoderSetLabel(Handle, m.RentUtf8Ptr(label));
    }

    public unsafe void SetPipeline(ComputePipelineHandle pipeline)
    {
        WebGPU.ComputePassEncoderSetPipeline(Handle, pipeline);
    }

    public unsafe void Reference()
    {
        WebGPU.ComputePassEncoderReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.ComputePassEncoderRelease(Handle);
    }

    public void Dispose()
    {
        if (Handle.IsEmpty)
            return;
        Disposing();
        Release();
        Handle = default;
        Disposed();
    }

    partial void Disposing();
    partial void Disposed();
}