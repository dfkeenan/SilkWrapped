namespace SilkWrapped.WebGPU;
using WebGPU = Silk.NET.WebGPU.WebGPU;
using QuerySet = Silk.NET.WebGPU.QuerySet;

public unsafe partial class QuerySetWrapper
{
    public QuerySetWrapper(WebGPU webGPU, WebGPUDisposal disposal, QuerySet* rawPointer)
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
    public QuerySet* RawPointer { get; }

    public void Destroy()
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.QuerySetDestroy(RawPointer);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public UInt32 GetCount()
    {
        if (WebGPU is WebGPU webGPU)
        {
            return webGPU.QuerySetGetCount(RawPointer);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public QueryType GetType()
    {
        if (WebGPU is WebGPU webGPU)
        {
            return webGPU.QuerySetGetType(RawPointer);
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
            webGPU.QuerySetSetLabel(RawPointer, label);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public static implicit operator QuerySet*(QuerySetWrapper wrapper) => wrapper.RawPointer;
}