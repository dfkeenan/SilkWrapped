namespace SilkWrapped.WebGPU;
using WebGPU = Silk.NET.WebGPU.WebGPU;
using RenderPassEncoder = Silk.NET.WebGPU.RenderPassEncoder;

public unsafe partial class RenderPassEncoderWrapper
{
    public RenderPassEncoderWrapper(WebGPU webGPU, WebGPUDisposal disposal, RenderPassEncoder* rawPointer)
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
    public RenderPassEncoder* RawPointer { get; }

    public void InsertDebugMarker(String markerLabel)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.RenderPassEncoderInsertDebugMarker(RawPointer, markerLabel);
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
            webGPU.RenderPassEncoderPopDebugGroup(RawPointer);
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
            webGPU.RenderPassEncoderPushDebugGroup(RawPointer, groupLabel);
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
            webGPU.RenderPassEncoderSetBindGroup(RawPointer, groupIndex, group, dynamicOffsetCount, dynamicOffsets);
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
            webGPU.RenderPassEncoderSetBindGroup(RawPointer, groupIndex, group, dynamicOffsetCount, dynamicOffsets);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void SetBlendConstant(in Color color)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.RenderPassEncoderSetBlendConstant(RawPointer, color);
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
            webGPU.RenderPassEncoderSetLabel(RawPointer, label);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void SetScissorRect(UInt32 x, UInt32 y, UInt32 width, UInt32 height)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.RenderPassEncoderSetScissorRect(RawPointer, x, y, width, height);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void SetStencilReference(UInt32 reference)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.RenderPassEncoderSetStencilReference(RawPointer, reference);
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
            webGPU.RenderPassEncoderSetVertexBuffer(RawPointer, slot, buffer, offset, size);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void SetViewport(Single x, Single y, Single width, Single height, Single minDepth, Single maxDepth)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.RenderPassEncoderSetViewport(RawPointer, x, y, width, height, minDepth, maxDepth);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void BeginOcclusionQuery(UInt32 queryIndex)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.RenderPassEncoderBeginOcclusionQuery(RawPointer, queryIndex);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void Draw(UInt32 vertexCount, UInt32 instanceCount, UInt32 firstVertex, UInt32 firstInstance)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.RenderPassEncoderDraw(RawPointer, vertexCount, instanceCount, firstVertex, firstInstance);
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
            webGPU.RenderPassEncoderDrawIndexed(RawPointer, indexCount, instanceCount, firstIndex, baseVertex, firstInstance);
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
            webGPU.RenderPassEncoderEnd(RawPointer);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void EndOcclusionQuery()
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.RenderPassEncoderEndOcclusionQuery(RawPointer);
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
            webGPU.RenderPassEncoderEndPipelineStatisticsQuery(RawPointer);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void ExecuteBundles(UInt32 bundleCount, RenderBundle** bundles)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.RenderPassEncoderExecuteBundles(RawPointer, bundleCount, bundles);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void ExecuteBundles(UInt32 bundleCount, ref RenderBundle* bundles)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.RenderPassEncoderExecuteBundles(RawPointer, bundleCount, ref bundles);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public static implicit operator RenderPassEncoder*(RenderPassEncoderWrapper wrapper) => wrapper.RawPointer;
}