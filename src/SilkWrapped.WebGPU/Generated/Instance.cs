using System.Diagnostics.CodeAnalysis;
using Silk.NET.Core;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct InstanceHandle : IEquatable<InstanceHandle>
{
    private readonly nint nativeHandle;
    private InstanceHandle(Silk.NET.WebGPU.Instance* nativeHandle)
    {
        this.nativeHandle = (nint)nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.Instance*(InstanceHandle handle) => (Silk.NET.WebGPU.Instance*)handle.nativeHandle;
    public static implicit operator InstanceHandle(Silk.NET.WebGPU.Instance* handle) => new InstanceHandle(handle);
    public static bool operator ==(InstanceHandle handle, InstanceHandle other) => handle.nativeHandle == other.nativeHandle;
    public static bool operator !=(InstanceHandle handle, InstanceHandle other) => handle.nativeHandle != other.nativeHandle;
    public bool Equals(InstanceHandle other) => this == other;
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not InstanceHandle other)
            return false;
        return this == other;
    }

    public override int GetHashCode() => (int)nativeHandle;
}

public unsafe partial class Instance : IEquatable<Instance>, System.IDisposable
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

    public bool Equals([NotNullWhen(true)] Instance? other)
    {
        if (other is null)
            return false;
        return Handle == other.Handle;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not Instance other)
            return false;
        return Handle == other.Handle;
    }

    public override int GetHashCode() => Handle.GetHashCode();
    public void  Dispose()
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