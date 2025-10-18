using System.Diagnostics.CodeAnalysis;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct RenderBundleHandle : IEquatable<RenderBundleHandle>
{
    private readonly nint nativeHandle;
    private RenderBundleHandle(Silk.NET.WebGPU.RenderBundle* nativeHandle)
    {
        this.nativeHandle = (nint)nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.RenderBundle*(RenderBundleHandle handle) => (Silk.NET.WebGPU.RenderBundle*)handle.nativeHandle;
    public static implicit operator RenderBundleHandle(Silk.NET.WebGPU.RenderBundle* handle) => new RenderBundleHandle(handle);
    public static bool operator ==(RenderBundleHandle handle, RenderBundleHandle other) => handle.nativeHandle == other.nativeHandle;
    public static bool operator !=(RenderBundleHandle handle, RenderBundleHandle other) => handle.nativeHandle != other.nativeHandle;
    public bool Equals(RenderBundleHandle other) => this == other;
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not RenderBundleHandle other)
            return false;
        return this == other;
    }

    public override int GetHashCode() => (int)nativeHandle;
}

public unsafe partial class RenderBundle : IEquatable<RenderBundle>, System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public RenderBundleHandle Handle { get; private set; }

    public RenderBundle(Silk.NET.WebGPU.WebGPU webGPU, RenderBundleHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator RenderBundleHandle(RenderBundle obj) => obj.Handle;
    public unsafe void SetLabel(string label)
    {
        using var m = new MarshalHelper();
        WebGPU.RenderBundleSetLabel(Handle, m.RentUtf8Ptr(label));
    }

    public unsafe void Reference()
    {
        WebGPU.RenderBundleReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.RenderBundleRelease(Handle);
    }

    public bool Equals([NotNullWhen(true)] RenderBundle? other)
    {
        if (other is null)
            return false;
        return Handle == other.Handle;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not RenderBundle other)
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