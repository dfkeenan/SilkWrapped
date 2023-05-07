namespace SilkWrapped.WebGPU;
using WebGPU = Silk.NET.WebGPU.WebGPU;
using TextureView = Silk.NET.WebGPU.TextureView;

public unsafe partial class TextureViewWrapper
{
    public TextureViewWrapper(WebGPU webGPU, WebGPUDisposal disposal, TextureView* rawPointer)
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
    public TextureView* RawPointer { get; }

    public void SetLabel(String label)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.TextureViewSetLabel(RawPointer, label);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public static implicit operator TextureView*(TextureViewWrapper wrapper) => wrapper.RawPointer;
}