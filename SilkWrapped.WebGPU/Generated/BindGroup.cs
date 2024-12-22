using System.Diagnostics.CodeAnalysis;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct BindGroupHandle : IEquatable<BindGroupHandle>
{
    private readonly nint nativeHandle;
    private BindGroupHandle(Silk.NET.WebGPU.BindGroup* nativeHandle)
    {
        this.nativeHandle = (nint)nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.BindGroup*(BindGroupHandle handle) => (Silk.NET.WebGPU.BindGroup*)handle.nativeHandle;
    public static implicit operator BindGroupHandle(Silk.NET.WebGPU.BindGroup* handle) => new BindGroupHandle(handle);
    public static bool operator ==(BindGroupHandle handle, BindGroupHandle other) => handle.nativeHandle == other.nativeHandle;
    public static bool operator !=(BindGroupHandle handle, BindGroupHandle other) => handle.nativeHandle != other.nativeHandle;
    public bool Equals(BindGroupHandle other) => this == other;
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not BindGroupHandle other)
            return false;
        return this == other;
    }

    public override int GetHashCode() => (int)nativeHandle;
}

public unsafe partial class BindGroup : IEquatable<BindGroup>, System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public BindGroupHandle Handle { get; private set; }

    public BindGroup(Silk.NET.WebGPU.WebGPU webGPU, BindGroupHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator BindGroupHandle(BindGroup obj) => obj.Handle;
    public unsafe void SetLabel(string label)
    {
        using var m = new MarshalHelper();
        WebGPU.BindGroupSetLabel(Handle, m.RentUtf8Ptr(label));
    }

    public unsafe void Reference()
    {
        WebGPU.BindGroupReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.BindGroupRelease(Handle);
    }

    public bool Equals([NotNullWhen(true)] BindGroup? other)
    {
        if (other is null)
            return false;
        return Handle == other.Handle;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not BindGroup other)
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