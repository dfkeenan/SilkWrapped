using System.Diagnostics.CodeAnalysis;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct BufferHandle : IEquatable<BufferHandle>
{
    private readonly nint nativeHandle;
    private BufferHandle(Silk.NET.WebGPU.Buffer* nativeHandle)
    {
        this.nativeHandle = (nint)nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.Buffer*(BufferHandle handle) => (Silk.NET.WebGPU.Buffer*)handle.nativeHandle;
    public static implicit operator BufferHandle(Silk.NET.WebGPU.Buffer* handle) => new BufferHandle(handle);
    public static bool operator ==(BufferHandle handle, BufferHandle other) => handle.nativeHandle == other.nativeHandle;
    public static bool operator !=(BufferHandle handle, BufferHandle other) => handle.nativeHandle != other.nativeHandle;
    public bool Equals(BufferHandle other) => this == other;
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not BufferHandle other)
            return false;
        return this == other;
    }

    public override int GetHashCode() => (int)nativeHandle;
}

public unsafe partial class Buffer : IEquatable<Buffer>, System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public BufferHandle Handle { get; private set; }

    public Buffer(Silk.NET.WebGPU.WebGPU webGPU, BufferHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator BufferHandle(Buffer obj) => obj.Handle;
    public unsafe void Destroy()
    {
        WebGPU.BufferDestroy(Handle);
    }

    public unsafe void* GetConstMappedRange(nuint offset, nuint size)
    {
        var result = WebGPU.BufferGetConstMappedRange(Handle, offset, size);
        return result;
    }

    public unsafe BufferMapState GetMapState()
    {
        var result = WebGPU.BufferGetMapState(Handle);
        return (BufferMapState)result;
    }

    public unsafe void* GetMappedRange(nuint offset, nuint size)
    {
        var result = WebGPU.BufferGetMappedRange(Handle, offset, size);
        return result;
    }

    public unsafe ulong GetSize()
    {
        var result = WebGPU.BufferGetSize(Handle);
        return result;
    }

    public unsafe BufferUsage GetUsage()
    {
        var result = WebGPU.BufferGetUsage(Handle);
        return (BufferUsage)result;
    }

    public unsafe void MapAsync(MapMode mode, nuint offset, nuint size, PfnBufferMapCallback callback)
    {
        WebGPU.BufferMapAsync(Handle, (Silk.NET.WebGPU.MapMode)mode, offset, size, callback, null);
    }

    public unsafe void MapAsync<T0>(MapMode mode, nuint offset, nuint size, PfnBufferMapCallback callback, ref T0 userdata)
        where T0 : unmanaged
    {
        WebGPU.BufferMapAsync<T0>(Handle, (Silk.NET.WebGPU.MapMode)mode, offset, size, callback, ref userdata);
    }

    public unsafe void SetLabel(string label)
    {
        using var m = new MarshalHelper();
        WebGPU.BufferSetLabel(Handle, m.RentUtf8Ptr(label));
    }

    public unsafe void Unmap()
    {
        WebGPU.BufferUnmap(Handle);
    }

    public unsafe void Reference()
    {
        WebGPU.BufferReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.BufferRelease(Handle);
    }

    public bool Equals([NotNullWhen(true)] Buffer? other)
    {
        if (other is null)
            return false;
        return Handle == other.Handle;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not Buffer other)
            return false;
        return Handle == other.Handle;
    }

    public override int GetHashCode() => Handle.GetHashCode();
    public void  Dispose()
    {
        if (Handle.IsEmpty)
            return;
        Disposing();
        Destroy();
        Handle = default;
        Disposed();
    }

    partial void Disposing();
    partial void Disposed();
}