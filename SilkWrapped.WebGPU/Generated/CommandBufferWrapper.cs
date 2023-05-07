namespace SilkWrapped.WebGPU;
using WebGPU = Silk.NET.WebGPU.WebGPU;
using CommandBuffer = Silk.NET.WebGPU.CommandBuffer;

public unsafe partial class CommandBufferWrapper
{
    public CommandBufferWrapper(WebGPU webGPU, WebGPUDisposal disposal, CommandBuffer* rawPointer)
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
    public CommandBuffer* RawPointer { get; }

    public void SetLabel(String label)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.CommandBufferSetLabel(RawPointer, label);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public static implicit operator CommandBuffer*(CommandBufferWrapper wrapper) => wrapper.RawPointer;
}