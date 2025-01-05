using System.Diagnostics.CodeAnalysis;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct PipelineLayoutHandle : IEquatable<PipelineLayoutHandle>
{
    private readonly nint nativeHandle;
    private PipelineLayoutHandle(Silk.NET.WebGPU.PipelineLayout* nativeHandle)
    {
        this.nativeHandle = (nint)nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.PipelineLayout*(PipelineLayoutHandle handle) => (Silk.NET.WebGPU.PipelineLayout*)handle.nativeHandle;
    public static implicit operator PipelineLayoutHandle(Silk.NET.WebGPU.PipelineLayout* handle) => new PipelineLayoutHandle(handle);
    public static bool operator ==(PipelineLayoutHandle handle, PipelineLayoutHandle other) => handle.nativeHandle == other.nativeHandle;
    public static bool operator !=(PipelineLayoutHandle handle, PipelineLayoutHandle other) => handle.nativeHandle != other.nativeHandle;
    public bool Equals(PipelineLayoutHandle other) => this == other;
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not PipelineLayoutHandle other)
            return false;
        return this == other;
    }

    public override int GetHashCode() => (int)nativeHandle;
}

public unsafe partial class PipelineLayout : IEquatable<PipelineLayout>, System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public PipelineLayoutHandle Handle { get; private set; }

    public PipelineLayout(Silk.NET.WebGPU.WebGPU webGPU, PipelineLayoutHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator PipelineLayoutHandle(PipelineLayout obj) => obj.Handle;
    public unsafe void SetLabel(string label)
    {
        using var m = new MarshalHelper();
        WebGPU.PipelineLayoutSetLabel(Handle, m.RentUtf8Ptr(label));
    }

    public unsafe void Reference()
    {
        WebGPU.PipelineLayoutReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.PipelineLayoutRelease(Handle);
    }

    public bool Equals([NotNullWhen(true)] PipelineLayout? other)
    {
        if (other is null)
            return false;
        return Handle == other.Handle;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not PipelineLayout other)
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