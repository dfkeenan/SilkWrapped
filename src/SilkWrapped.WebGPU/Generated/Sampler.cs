using System.Diagnostics.CodeAnalysis;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct SamplerHandle : IEquatable<SamplerHandle>
{
    private readonly nint nativeHandle;
    private SamplerHandle(Silk.NET.WebGPU.Sampler* nativeHandle)
    {
        this.nativeHandle = (nint)nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.Sampler*(SamplerHandle handle) => (Silk.NET.WebGPU.Sampler*)handle.nativeHandle;
    public static implicit operator SamplerHandle(Silk.NET.WebGPU.Sampler* handle) => new SamplerHandle(handle);
    public static bool operator ==(SamplerHandle handle, SamplerHandle other) => handle.nativeHandle == other.nativeHandle;
    public static bool operator !=(SamplerHandle handle, SamplerHandle other) => handle.nativeHandle != other.nativeHandle;
    public bool Equals(SamplerHandle other) => this == other;
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not SamplerHandle other)
            return false;
        return this == other;
    }

    public override int GetHashCode() => (int)nativeHandle;
}

public unsafe partial class Sampler : IEquatable<Sampler>, System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public SamplerHandle Handle { get; private set; }

    public Sampler(Silk.NET.WebGPU.WebGPU webGPU, SamplerHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator SamplerHandle(Sampler obj) => obj.Handle;
    public unsafe void SetLabel(string label)
    {
        using var m = new MarshalHelper();
        WebGPU.SamplerSetLabel(Handle, m.RentUtf8Ptr(label));
    }

    public unsafe void Reference()
    {
        WebGPU.SamplerReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.SamplerRelease(Handle);
    }

    public bool Equals([NotNullWhen(true)] Sampler? other)
    {
        if (other is null)
            return false;
        return Handle == other.Handle;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not Sampler other)
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