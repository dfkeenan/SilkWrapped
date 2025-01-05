using System.Diagnostics.CodeAnalysis;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct BindGroupLayoutHandle : IEquatable<BindGroupLayoutHandle>
{
    private readonly nint nativeHandle;
    private BindGroupLayoutHandle(Silk.NET.WebGPU.BindGroupLayout* nativeHandle)
    {
        this.nativeHandle = (nint)nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.BindGroupLayout*(BindGroupLayoutHandle handle) => (Silk.NET.WebGPU.BindGroupLayout*)handle.nativeHandle;
    public static implicit operator BindGroupLayoutHandle(Silk.NET.WebGPU.BindGroupLayout* handle) => new BindGroupLayoutHandle(handle);
    public static bool operator ==(BindGroupLayoutHandle handle, BindGroupLayoutHandle other) => handle.nativeHandle == other.nativeHandle;
    public static bool operator !=(BindGroupLayoutHandle handle, BindGroupLayoutHandle other) => handle.nativeHandle != other.nativeHandle;
    public bool Equals(BindGroupLayoutHandle other) => this == other;
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not BindGroupLayoutHandle other)
            return false;
        return this == other;
    }

    public override int GetHashCode() => (int)nativeHandle;
}

public unsafe partial class BindGroupLayout : IEquatable<BindGroupLayout>, System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public BindGroupLayoutHandle Handle { get; private set; }

    public BindGroupLayout(Silk.NET.WebGPU.WebGPU webGPU, BindGroupLayoutHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator BindGroupLayoutHandle(BindGroupLayout obj) => obj.Handle;
    public unsafe void SetLabel(string label)
    {
        using var m = new MarshalHelper();
        WebGPU.BindGroupLayoutSetLabel(Handle, m.RentUtf8Ptr(label));
    }

    public unsafe void Reference()
    {
        WebGPU.BindGroupLayoutReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.BindGroupLayoutRelease(Handle);
    }

    public bool Equals([NotNullWhen(true)] BindGroupLayout? other)
    {
        if (other is null)
            return false;
        return Handle == other.Handle;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not BindGroupLayout other)
            return false;
        return Handle == other.Handle;
    }

    public override int GetHashCode() => Handle.GetHashCode();
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