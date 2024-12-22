using System.Diagnostics.CodeAnalysis;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct CommandBufferHandle : IEquatable<CommandBufferHandle>
{
    private readonly nint nativeHandle;
    private CommandBufferHandle(Silk.NET.WebGPU.CommandBuffer* nativeHandle)
    {
        this.nativeHandle = (nint)nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.CommandBuffer*(CommandBufferHandle handle) => (Silk.NET.WebGPU.CommandBuffer*)handle.nativeHandle;
    public static implicit operator CommandBufferHandle(Silk.NET.WebGPU.CommandBuffer* handle) => new CommandBufferHandle(handle);
    public static bool operator ==(CommandBufferHandle handle, CommandBufferHandle other) => handle.nativeHandle == other.nativeHandle;
    public static bool operator !=(CommandBufferHandle handle, CommandBufferHandle other) => handle.nativeHandle != other.nativeHandle;
    public bool Equals(CommandBufferHandle other) => this == other;
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not CommandBufferHandle other)
            return false;
        return this == other;
    }

    public override int GetHashCode() => (int)nativeHandle;
}

public unsafe partial class CommandBuffer : IEquatable<CommandBuffer>, System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public CommandBufferHandle Handle { get; private set; }

    public CommandBuffer(Silk.NET.WebGPU.WebGPU webGPU, CommandBufferHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator CommandBufferHandle(CommandBuffer obj) => obj.Handle;
    public unsafe void SetLabel(string label)
    {
        using var m = new MarshalHelper();
        WebGPU.CommandBufferSetLabel(Handle, m.RentUtf8Ptr(label));
    }

    public unsafe void Reference()
    {
        WebGPU.CommandBufferReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.CommandBufferRelease(Handle);
    }

    public bool Equals([NotNullWhen(true)] CommandBuffer? other)
    {
        if (other is null)
            return false;
        return Handle == other.Handle;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not CommandBuffer other)
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