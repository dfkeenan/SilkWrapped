using Silk.NET.Core;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct InstanceHandle
{
    private readonly Silk.NET.WebGPU.Instance* nativeHandle;
    private InstanceHandle(Silk.NET.WebGPU.Instance* nativeHandle)
    {
        this.nativeHandle = nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.Instance*(InstanceHandle handle) => handle.nativeHandle;
    public static implicit operator InstanceHandle(Silk.NET.WebGPU.Instance* handle) => new InstanceHandle(handle);
}

public unsafe partial class Instance : System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public InstanceHandle Handle { get; private set; }

    public Instance(Silk.NET.WebGPU.WebGPU webGPU, InstanceHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator InstanceHandle(Instance obj) => obj.Handle;
    public unsafe Surface CreateSurface(string? label = null)
    {
        using var m = new MarshalHelper();
        Silk.NET.WebGPU.SurfaceDescriptor __descriptor = default;
        __descriptor.Label = m.RentUtf8Ptr(label);
        var result = WebGPU.InstanceCreateSurface(Handle, in __descriptor);
        return new Surface(WebGPU, result);
    }

    public unsafe Bool32 HasWGSLLanguageFeature(WGSLFeatureName feature)
    {
        var result = WebGPU.InstanceHasWGSLLanguageFeature(Handle, (Silk.NET.WebGPU.WGSLFeatureName)feature);
        return result;
    }

    public unsafe void ProcessEvents()
    {
        WebGPU.InstanceProcessEvents(Handle);
    }

    public unsafe void RequestAdapter(in RequestAdapterOptions options, PfnRequestAdapterCallback callback)
    {
        Silk.NET.WebGPU.RequestAdapterOptions __options = default;
        __options.CompatibleSurface = options.CompatibleSurface;
        __options.PowerPreference = (Silk.NET.WebGPU.PowerPreference)options.PowerPreference;
        __options.BackendType = (Silk.NET.WebGPU.BackendType)options.BackendType;
        __options.ForceFallbackAdapter = options.ForceFallbackAdapter;
        WebGPU.InstanceRequestAdapter(Handle, in __options, callback, null);
    }

    public unsafe void RequestAdapter<T0>(in RequestAdapterOptions options, PfnRequestAdapterCallback callback, ref T0 userdata)
        where T0 : unmanaged
    {
        Silk.NET.WebGPU.RequestAdapterOptions __options = default;
        __options.CompatibleSurface = options.CompatibleSurface;
        __options.PowerPreference = (Silk.NET.WebGPU.PowerPreference)options.PowerPreference;
        __options.BackendType = (Silk.NET.WebGPU.BackendType)options.BackendType;
        __options.ForceFallbackAdapter = options.ForceFallbackAdapter;
        WebGPU.InstanceRequestAdapter<T0>(Handle, in __options, callback, ref userdata);
    }

    public unsafe void Reference()
    {
        WebGPU.InstanceReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.InstanceRelease(Handle);
    }

    public void Dispose()
    {
        if (Handle.IsEmpty)
            return;
        Disposing();
        Release();
        Handle = default;
        Disposed();
    }

    partial void Disposing();
    partial void Disposed();
}