namespace SilkWrapped.WebGPU;
using WebGPU = Silk.NET.WebGPU.WebGPU;
using Queue = Silk.NET.WebGPU.Queue;

public unsafe partial class QueueWrapper
{
    public QueueWrapper(WebGPU webGPU, WebGPUDisposal disposal, Queue* rawPointer)
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
    public Queue* RawPointer { get; }

    public void OnSubmittedWorkDone<T0>(PfnQueueWorkDoneCallback callback, ref T0 userdata)
        where T0 : unmanaged
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.QueueOnSubmittedWorkDone(RawPointer, callback, ref userdata);
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
            webGPU.QueueSetLabel(RawPointer, label);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void Submit(UInt32 commandCount, CommandBuffer** commands)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.QueueSubmit(RawPointer, commandCount, commands);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void Submit(UInt32 commandCount, ref CommandBuffer* commands)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.QueueSubmit(RawPointer, commandCount, ref commands);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void WriteTexture<T0>(in ImageCopyTexture destination, in T0 data, UIntPtr dataSize, TextureDataLayout* dataLayout, Extent3D* writeSize)
        where T0 : unmanaged
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.QueueWriteTexture(RawPointer, destination, data, dataSize, dataLayout, writeSize);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void WriteTexture<T0>(in ImageCopyTexture destination, in T0 data, UIntPtr dataSize, TextureDataLayout* dataLayout, in Extent3D writeSize)
        where T0 : unmanaged
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.QueueWriteTexture(RawPointer, destination, data, dataSize, dataLayout, writeSize);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void WriteTexture<T0>(in ImageCopyTexture destination, in T0 data, UIntPtr dataSize, in TextureDataLayout dataLayout, Extent3D* writeSize)
        where T0 : unmanaged
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.QueueWriteTexture(RawPointer, destination, data, dataSize, dataLayout, writeSize);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void WriteTexture<T0>(in ImageCopyTexture destination, in T0 data, UIntPtr dataSize, in TextureDataLayout dataLayout, in Extent3D writeSize)
        where T0 : unmanaged
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.QueueWriteTexture(RawPointer, destination, data, dataSize, dataLayout, writeSize);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public static implicit operator Queue*(QueueWrapper wrapper) => wrapper.RawPointer;
}