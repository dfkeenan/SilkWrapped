namespace SilkWrapped.WebGPU;
using WebGPU = Silk.NET.WebGPU.WebGPU;
using CommandEncoder = Silk.NET.WebGPU.CommandEncoder;

public unsafe partial class CommandEncoderWrapper
{
    public CommandEncoderWrapper(WebGPU webGPU, WebGPUDisposal disposal, CommandEncoder* rawPointer)
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
    public CommandEncoder* RawPointer { get; }

    public ComputePassEncoderWrapper BeginComputePass(in ComputePassDescriptor descriptor)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return new ComputePassEncoderWrapper(WebGPU, Disposal, webGPU.CommandEncoderBeginComputePass(RawPointer, descriptor));
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public RenderPassEncoderWrapper BeginRenderPass(in RenderPassDescriptor descriptor)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return new RenderPassEncoderWrapper(WebGPU, Disposal, webGPU.CommandEncoderBeginRenderPass(RawPointer, descriptor));
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void CopyBufferToTexture(in ImageCopyBuffer source, ImageCopyTexture* destination, Extent3D* copySize)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.CommandEncoderCopyBufferToTexture(RawPointer, source, destination, copySize);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void CopyBufferToTexture(in ImageCopyBuffer source, ImageCopyTexture* destination, in Extent3D copySize)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.CommandEncoderCopyBufferToTexture(RawPointer, source, destination, copySize);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void CopyBufferToTexture(in ImageCopyBuffer source, in ImageCopyTexture destination, Extent3D* copySize)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.CommandEncoderCopyBufferToTexture(RawPointer, source, destination, copySize);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void CopyBufferToTexture(in ImageCopyBuffer source, in ImageCopyTexture destination, in Extent3D copySize)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.CommandEncoderCopyBufferToTexture(RawPointer, source, destination, copySize);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void CopyTextureToBuffer(in ImageCopyTexture source, ImageCopyBuffer* destination, Extent3D* copySize)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.CommandEncoderCopyTextureToBuffer(RawPointer, source, destination, copySize);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void CopyTextureToBuffer(in ImageCopyTexture source, ImageCopyBuffer* destination, in Extent3D copySize)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.CommandEncoderCopyTextureToBuffer(RawPointer, source, destination, copySize);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void CopyTextureToBuffer(in ImageCopyTexture source, in ImageCopyBuffer destination, Extent3D* copySize)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.CommandEncoderCopyTextureToBuffer(RawPointer, source, destination, copySize);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void CopyTextureToBuffer(in ImageCopyTexture source, in ImageCopyBuffer destination, in Extent3D copySize)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.CommandEncoderCopyTextureToBuffer(RawPointer, source, destination, copySize);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void CopyTextureToTexture(in ImageCopyTexture source, ImageCopyTexture* destination, Extent3D* copySize)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.CommandEncoderCopyTextureToTexture(RawPointer, source, destination, copySize);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void CopyTextureToTexture(in ImageCopyTexture source, ImageCopyTexture* destination, in Extent3D copySize)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.CommandEncoderCopyTextureToTexture(RawPointer, source, destination, copySize);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void CopyTextureToTexture(in ImageCopyTexture source, in ImageCopyTexture destination, Extent3D* copySize)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.CommandEncoderCopyTextureToTexture(RawPointer, source, destination, copySize);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void CopyTextureToTexture(in ImageCopyTexture source, in ImageCopyTexture destination, in Extent3D copySize)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.CommandEncoderCopyTextureToTexture(RawPointer, source, destination, copySize);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public CommandBufferWrapper Finish(in CommandBufferDescriptor descriptor)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return new CommandBufferWrapper(WebGPU, Disposal, webGPU.CommandEncoderFinish(RawPointer, descriptor));
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
            webGPU.CommandEncoderInsertDebugMarker(RawPointer, markerLabel);
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
            webGPU.CommandEncoderPopDebugGroup(RawPointer);
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
            webGPU.CommandEncoderPushDebugGroup(RawPointer, groupLabel);
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
            webGPU.CommandEncoderSetLabel(RawPointer, label);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public static implicit operator CommandEncoder*(CommandEncoderWrapper wrapper) => wrapper.RawPointer;
}