namespace SilkWrapped.WebGPU;
using WebGPU = Silk.NET.WebGPU.WebGPU;
using Adapter = Silk.NET.WebGPU.Adapter;

public unsafe partial class AdapterWrapper
{
    public AdapterWrapper(WebGPU webGPU, WebGPUDisposal disposal, Adapter* rawPointer)
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
    public Adapter* RawPointer { get; }

    public UIntPtr EnumerateFeatures(ref FeatureName features)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return webGPU.AdapterEnumerateFeatures(RawPointer, ref features);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public Bool32 GetLimits(ref SupportedLimits limits)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return webGPU.AdapterGetLimits(RawPointer, ref limits);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void GetProperties(ref AdapterProperties properties)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.AdapterGetProperties(RawPointer, ref properties);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public Bool32 HasFeature(FeatureName feature)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return webGPU.AdapterHasFeature(RawPointer, feature);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void RequestDevice<T0>(in DeviceDescriptor descriptor, PfnRequestDeviceCallback callback, ref T0 userdata)
        where T0 : unmanaged
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.AdapterRequestDevice(RawPointer, descriptor, callback, ref userdata);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public static implicit operator Adapter*(AdapterWrapper wrapper) => wrapper.RawPointer;
}