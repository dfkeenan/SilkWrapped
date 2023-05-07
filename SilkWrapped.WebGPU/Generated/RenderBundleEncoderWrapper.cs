namespace SilkWrapped.WebGPU;
using WebGPU = Silk.NET.WebGPU.WebGPU;
using RenderBundleEncoder = Silk.NET.WebGPU.RenderBundleEncoder;

public unsafe partial class RenderBundleEncoderWrapper
{
    public RenderBundleEncoderWrapper(WebGPU webGPU, WebGPUDisposal disposal, RenderBundleEncoder* rawPointer)
    {
        weakWebGPUReference = new WeakReference<WebGPU>(webGPU ?? throw new ArgumentNullException(nameof(webGPU)));
        weakWebGPUDisposalReference = new WeakReference<WebGPUDisposal>(disposal ?? throw new ArgumentNullException(nameof(disposal)));
        this.RawPointer = rawPointer == null ? rawPointer : throw new ArgumentNullException(nameof(rawPointer));
    }

    WeakReference<WebGPU> weakWebGPUReference;
    [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
    public WebGPU? WebGPU => weakWebGPUReference.TryGetTarget(out var target) ? target : null;
    WeakReference<WebGPUDisposal> weakWebGPUDisposalReference;
    [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
    public WebGPUDisposal? Disposal => weakWebGPUDisposalReference.TryGetTarget(out var target) ? target : null;
    [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
    public RenderBundleEncoder* RawPointer { get; }

    public void Draw(UInt32 vertexCount, UInt32 instanceCount, UInt32 firstVertex, UInt32 firstInstance)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.RenderBundleEncoderDraw(RawPointer, vertexCount, instanceCount, firstVertex, firstInstance);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void DrawIndexed(UInt32 indexCount, UInt32 instanceCount, UInt32 firstIndex, Int32 baseVertex, UInt32 firstInstance)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.RenderBundleEncoderDrawIndexed(RawPointer, indexCount, instanceCount, firstIndex, baseVertex, firstInstance);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public RenderBundleWrapper Finish(in RenderBundleDescriptor descriptor)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return new RenderBundleWrapper(WebGPU, Disposal, webGPU.RenderBundleEncoderFinish(RawPointer, descriptor));
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void InsertDebugMarker(String markerLabel)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.RenderBundleEncoderInsertDebugMarker(RawPointer, markerLabel);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void PopDebugGroup()
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.RenderBundleEncoderPopDebugGroup(RawPointer);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void PushDebugGroup(String groupLabel)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.RenderBundleEncoderPushDebugGroup(RawPointer, groupLabel);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void SetBindGroup(UInt32 groupIndex, BindGroup* group, UInt32 dynamicOffsetCount, UInt32* dynamicOffsets)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.RenderBundleEncoderSetBindGroup(RawPointer, groupIndex, group, dynamicOffsetCount, dynamicOffsets);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void SetBindGroup(UInt32 groupIndex, BindGroup* group, UInt32 dynamicOffsetCount, in UInt32 dynamicOffsets)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.RenderBundleEncoderSetBindGroup(RawPointer, groupIndex, group, dynamicOffsetCount, dynamicOffsets);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void SetLabel(String label)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.RenderBundleEncoderSetLabel(RawPointer, label);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void SetVertexBuffer(UInt32 slot, Buffer* buffer, UInt64 offset, UInt64 size)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.RenderBundleEncoderSetVertexBuffer(RawPointer, slot, buffer, offset, size);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public static implicit operator RenderBundleEncoder*(RenderBundleEncoderWrapper wrapper) => wrapper.RawPointer;
}