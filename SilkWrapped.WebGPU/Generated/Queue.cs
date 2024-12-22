using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct QueueHandle : IEquatable<QueueHandle>
{
    private readonly nint nativeHandle;
    private QueueHandle(Silk.NET.WebGPU.Queue* nativeHandle)
    {
        this.nativeHandle = (nint)nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.Queue*(QueueHandle handle) => (Silk.NET.WebGPU.Queue*)handle.nativeHandle;
    public static implicit operator QueueHandle(Silk.NET.WebGPU.Queue* handle) => new QueueHandle(handle);
    public static bool operator ==(QueueHandle handle, QueueHandle other) => handle.nativeHandle == other.nativeHandle;
    public static bool operator !=(QueueHandle handle, QueueHandle other) => handle.nativeHandle != other.nativeHandle;
    public bool Equals(QueueHandle other) => this == other;
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not QueueHandle other)
            return false;
        return this == other;
    }

    public override int GetHashCode() => (int)nativeHandle;
}

public unsafe partial class Queue : IEquatable<Queue>, System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public QueueHandle Handle { get; private set; }

    public Queue(Silk.NET.WebGPU.WebGPU webGPU, QueueHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator QueueHandle(Queue obj) => obj.Handle;
    public unsafe void OnSubmittedWorkDone(PfnQueueWorkDoneCallback callback)
    {
        WebGPU.QueueOnSubmittedWorkDone(Handle, callback, null);
    }

    public unsafe void OnSubmittedWorkDone<T0>(PfnQueueWorkDoneCallback callback, ref T0 userdata)
        where T0 : unmanaged
    {
        WebGPU.QueueOnSubmittedWorkDone<T0>(Handle, callback, ref userdata);
    }

    public unsafe void SetLabel(string label)
    {
        using var m = new MarshalHelper();
        WebGPU.QueueSetLabel(Handle, m.RentUtf8Ptr(label));
    }

    public unsafe void Submit(params ReadOnlySpan<CommandBufferHandle> commands)
    {
        fixed (CommandBufferHandle* commandsPtr = commands)
        {
            WebGPU.QueueSubmit(Handle, (nuint)commands.Length, (Silk.NET.WebGPU.CommandBuffer**)commandsPtr);
        }
    }

    public unsafe void WriteBuffer<T0>(BufferHandle buffer, ulong bufferOffset, in T0 data, nuint size)
        where T0 : unmanaged
    {
        WebGPU.QueueWriteBuffer<T0>(Handle, buffer, bufferOffset, in data, size);
    }

    public unsafe void WriteTexture<T0>(in ImageCopyTexture destination, in T0 data, nuint dataSize, in TextureDataLayout dataLayout, in Extent3D writeSize)
        where T0 : unmanaged
    {
        Silk.NET.WebGPU.ImageCopyTexture __destination = default;
        __destination.Texture = destination.Texture;
        __destination.MipLevel = destination.MipLevel;
        __destination.Origin = Unsafe.BitCast<Origin3D, Silk.NET.WebGPU.Origin3D>(destination.Origin);
        __destination.Aspect = (Silk.NET.WebGPU.TextureAspect)destination.Aspect;
        Silk.NET.WebGPU.TextureDataLayout __dataLayout = default;
        __dataLayout.Offset = dataLayout.Offset;
        __dataLayout.BytesPerRow = dataLayout.BytesPerRow;
        __dataLayout.RowsPerImage = dataLayout.RowsPerImage;
        Silk.NET.WebGPU.Extent3D __writeSize = default;
        __writeSize = Unsafe.BitCast<Extent3D, Silk.NET.WebGPU.Extent3D>(writeSize);
        WebGPU.QueueWriteTexture<T0>(Handle, in __destination, in data, dataSize, in __dataLayout, in __writeSize);
    }

    public unsafe void Reference()
    {
        WebGPU.QueueReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.QueueRelease(Handle);
    }

    public bool Equals([NotNullWhen(true)] Queue? other)
    {
        if (other is null)
            return false;
        return Handle == other.Handle;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not Queue other)
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