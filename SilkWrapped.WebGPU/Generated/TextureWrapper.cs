namespace SilkWrapped.WebGPU;
using WebGPU = Silk.NET.WebGPU.WebGPU;
using Texture = Silk.NET.WebGPU.Texture;

public unsafe partial class TextureWrapper
{
    public TextureWrapper(WebGPU webGPU, WebGPUDisposal disposal, Texture* rawPointer)
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
    public Texture* RawPointer { get; }

    public TextureViewWrapper CreateView(in TextureViewDescriptor descriptor)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return new TextureViewWrapper(WebGPU, Disposal, webGPU.TextureCreateView(RawPointer, descriptor));
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void Destroy()
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.TextureDestroy(RawPointer);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public UInt32 GetDepthOrArrayLayers()
    {
        if (WebGPU is WebGPU webGPU)
        {
            return webGPU.TextureGetDepthOrArrayLayers(RawPointer);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public TextureDimension GetDimension()
    {
        if (WebGPU is WebGPU webGPU)
        {
            return webGPU.TextureGetDimension(RawPointer);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public TextureFormat GetFormat()
    {
        if (WebGPU is WebGPU webGPU)
        {
            return webGPU.TextureGetFormat(RawPointer);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public UInt32 GetHeight()
    {
        if (WebGPU is WebGPU webGPU)
        {
            return webGPU.TextureGetHeight(RawPointer);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public UInt32 GetMipLevelCount()
    {
        if (WebGPU is WebGPU webGPU)
        {
            return webGPU.TextureGetMipLevelCount(RawPointer);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public UInt32 GetSampleCount()
    {
        if (WebGPU is WebGPU webGPU)
        {
            return webGPU.TextureGetSampleCount(RawPointer);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public TextureUsage GetUsage()
    {
        if (WebGPU is WebGPU webGPU)
        {
            return webGPU.TextureGetUsage(RawPointer);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public UInt32 GetWidth()
    {
        if (WebGPU is WebGPU webGPU)
        {
            return webGPU.TextureGetWidth(RawPointer);
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
            webGPU.TextureSetLabel(RawPointer, label);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public static implicit operator Texture*(TextureWrapper wrapper) => wrapper.RawPointer;
}