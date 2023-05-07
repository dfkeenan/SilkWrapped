namespace SilkWrapped.WebGPU;
using WebGPU = Silk.NET.WebGPU.WebGPU;
using SwapChain = Silk.NET.WebGPU.SwapChain;

public unsafe partial class SwapChainWrapper
{
    public SwapChainWrapper(WebGPU webGPU, WebGPUDisposal disposal, SwapChain* rawPointer)
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
    public SwapChain* RawPointer { get; }

    public TextureViewWrapper GetCurrentTextureView()
    {
        if (WebGPU is WebGPU webGPU)
        {
            return new TextureViewWrapper(WebGPU, Disposal, webGPU.SwapChainGetCurrentTextureView(RawPointer));
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void Present()
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.SwapChainPresent(RawPointer);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public static implicit operator SwapChain*(SwapChainWrapper wrapper) => wrapper.RawPointer;
}