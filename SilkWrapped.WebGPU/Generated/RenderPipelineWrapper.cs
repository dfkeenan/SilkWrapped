namespace SilkWrapped.WebGPU;
using WebGPU = Silk.NET.WebGPU.WebGPU;
using RenderPipeline = Silk.NET.WebGPU.RenderPipeline;

public unsafe partial class RenderPipelineWrapper
{
    public RenderPipelineWrapper(WebGPU webGPU, WebGPUDisposal disposal, RenderPipeline* rawPointer)
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
    public RenderPipeline* RawPointer { get; }

    public BindGroupLayoutWrapper GetBindGroupLayout(UInt32 groupIndex)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return new BindGroupLayoutWrapper(WebGPU, Disposal, webGPU.RenderPipelineGetBindGroupLayout(RawPointer, groupIndex));
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
            webGPU.RenderPipelineSetLabel(RawPointer, label);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public static implicit operator RenderPipeline*(RenderPipelineWrapper wrapper) => wrapper.RawPointer;
}