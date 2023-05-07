namespace SilkWrapped.WebGPU;
using WebGPU = Silk.NET.WebGPU.WebGPU;
using Buffer = Silk.NET.WebGPU.Buffer;

public unsafe partial class BufferWrapper
{
    public BufferWrapper(WebGPU webGPU, WebGPUDisposal disposal, Buffer* rawPointer)
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
    public Buffer* RawPointer { get; }

    public void Destroy()
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.BufferDestroy(RawPointer);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public ReadOnlySpan<byte> GetConstMappedRange(UIntPtr offset, UIntPtr size)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return new ReadOnlySpan<byte>(webGPU.BufferGetConstMappedRange(RawPointer, offset, size), (int)size);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public BufferMapState GetMapState()
    {
        if (WebGPU is WebGPU webGPU)
        {
            return webGPU.BufferGetMapState(RawPointer);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public ReadOnlySpan<byte> GetMappedRange(UIntPtr offset, UIntPtr size)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return new ReadOnlySpan<byte>(webGPU.BufferGetMappedRange(RawPointer, offset, size), (int)size);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public UInt64 GetSize()
    {
        if (WebGPU is WebGPU webGPU)
        {
            return webGPU.BufferGetSize(RawPointer);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public BufferUsage GetUsage()
    {
        if (WebGPU is WebGPU webGPU)
        {
            return webGPU.BufferGetUsage(RawPointer);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void MapAsync<T0>(MapMode mode, UIntPtr offset, UIntPtr size, PfnBufferMapCallback callback, ref T0 userdata)
        where T0 : unmanaged
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.BufferMapAsync(RawPointer, mode, offset, size, callback, ref userdata);
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
            webGPU.BufferSetLabel(RawPointer, label);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void Unmap()
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.BufferUnmap(RawPointer);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public static implicit operator Buffer*(BufferWrapper wrapper) => wrapper.RawPointer;
}