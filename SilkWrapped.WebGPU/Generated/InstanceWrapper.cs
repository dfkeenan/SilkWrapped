namespace SilkWrapped.WebGPU;
using WebGPU = Silk.NET.WebGPU.WebGPU;
using Instance = Silk.NET.WebGPU.Instance;

public unsafe partial class InstanceWrapper
{
    public InstanceWrapper(WebGPU webGPU, WebGPUDisposal disposal, Instance* rawPointer)
    {
        this.WebGPU = webGPU ?? throw new ArgumentNullException(nameof(webGPU));
        this.Disposal = disposal ?? throw new ArgumentNullException(nameof(disposal));
        this.RawPointer = rawPointer == null ? rawPointer : throw new ArgumentNullException(nameof(rawPointer));
    }

    [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
    public WebGPU WebGPU { get; }

    [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
    public WebGPUDisposal Disposal { get; }

    [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
    public Instance* RawPointer { get; }

    public SurfaceWrapper CreateSurface(in SurfaceDescriptor descriptor)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return new SurfaceWrapper(WebGPU, Disposal, webGPU.InstanceCreateSurface(RawPointer, descriptor));
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void ProcessEvents()
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.InstanceProcessEvents(RawPointer);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void RequestAdapter<T0>(in RequestAdapterOptions options, PfnRequestAdapterCallback callback, ref T0 userdata)
        where T0 : unmanaged
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.InstanceRequestAdapter(RawPointer, options, callback, ref userdata);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public static implicit operator Instance*(InstanceWrapper wrapper) => wrapper.RawPointer;
}