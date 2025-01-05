using System.Diagnostics.CodeAnalysis;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct ShaderModuleHandle : IEquatable<ShaderModuleHandle>
{
    private readonly nint nativeHandle;
    private ShaderModuleHandle(Silk.NET.WebGPU.ShaderModule* nativeHandle)
    {
        this.nativeHandle = (nint)nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.ShaderModule*(ShaderModuleHandle handle) => (Silk.NET.WebGPU.ShaderModule*)handle.nativeHandle;
    public static implicit operator ShaderModuleHandle(Silk.NET.WebGPU.ShaderModule* handle) => new ShaderModuleHandle(handle);
    public static bool operator ==(ShaderModuleHandle handle, ShaderModuleHandle other) => handle.nativeHandle == other.nativeHandle;
    public static bool operator !=(ShaderModuleHandle handle, ShaderModuleHandle other) => handle.nativeHandle != other.nativeHandle;
    public bool Equals(ShaderModuleHandle other) => this == other;
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not ShaderModuleHandle other)
            return false;
        return this == other;
    }

    public override int GetHashCode() => (int)nativeHandle;
}

public unsafe partial class ShaderModule : IEquatable<ShaderModule>, System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public ShaderModuleHandle Handle { get; private set; }

    public ShaderModule(Silk.NET.WebGPU.WebGPU webGPU, ShaderModuleHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator ShaderModuleHandle(ShaderModule obj) => obj.Handle;
    public unsafe void GetCompilationInfo(PfnCompilationInfoCallback callback)
    {
        WebGPU.ShaderModuleGetCompilationInfo(Handle, callback, null);
    }

    public unsafe void GetCompilationInfo<T0>(PfnCompilationInfoCallback callback, ref T0 userdata)
        where T0 : unmanaged
    {
        WebGPU.ShaderModuleGetCompilationInfo<T0>(Handle, callback, ref userdata);
    }

    public unsafe void SetLabel(string label)
    {
        using var m = new MarshalHelper();
        WebGPU.ShaderModuleSetLabel(Handle, m.RentUtf8Ptr(label));
    }

    public unsafe void Reference()
    {
        WebGPU.ShaderModuleReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.ShaderModuleRelease(Handle);
    }

    public bool Equals([NotNullWhen(true)] ShaderModule? other)
    {
        if (other is null)
            return false;
        return Handle == other.Handle;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not ShaderModule other)
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