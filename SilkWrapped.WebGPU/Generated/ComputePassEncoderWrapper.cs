namespace SilkWrapped.WebGPU;
using WebGPU = Silk.NET.WebGPU.WebGPU;
using ComputePassEncoder = Silk.NET.WebGPU.ComputePassEncoder;

public unsafe partial class ComputePassEncoderWrapper
{
    public ComputePassEncoderWrapper(WebGPU webGPU, WebGPUDisposal disposal, ComputePassEncoder* rawPointer)
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
    public ComputePassEncoder* RawPointer { get; }

    public void DispatchWorkgroups(UInt32 workgroupCountX, UInt32 workgroupCountY, UInt32 workgroupCountZ)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.ComputePassEncoderDispatchWorkgroups(RawPointer, workgroupCountX, workgroupCountY, workgroupCountZ);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void End()
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.ComputePassEncoderEnd(RawPointer);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void EndPipelineStatisticsQuery()
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.ComputePassEncoderEndPipelineStatisticsQuery(RawPointer);
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
            webGPU.ComputePassEncoderInsertDebugMarker(RawPointer, markerLabel);
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
            webGPU.ComputePassEncoderPopDebugGroup(RawPointer);
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
            webGPU.ComputePassEncoderPushDebugGroup(RawPointer, groupLabel);
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
            webGPU.ComputePassEncoderSetBindGroup(RawPointer, groupIndex, group, dynamicOffsetCount, dynamicOffsets);
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
            webGPU.ComputePassEncoderSetBindGroup(RawPointer, groupIndex, group, dynamicOffsetCount, dynamicOffsets);
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
            webGPU.ComputePassEncoderSetLabel(RawPointer, label);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public static implicit operator ComputePassEncoder*(ComputePassEncoderWrapper wrapper) => wrapper.RawPointer;
}