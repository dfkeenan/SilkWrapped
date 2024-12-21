using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using Silk.NET.Core;
using Silk.NET.Core.Attributes;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct CommandBufferHandle
{
    private readonly Silk.NET.WebGPU.CommandBuffer* nativeHandle;
    private CommandBufferHandle(Silk.NET.WebGPU.CommandBuffer* nativeHandle)
    {
        this.nativeHandle = nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.CommandBuffer*(CommandBufferHandle handle) => handle.nativeHandle;
    public static implicit operator CommandBufferHandle(Silk.NET.WebGPU.CommandBuffer* handle) => new CommandBufferHandle(handle);
}

public unsafe partial class CommandBuffer : System.IDisposable
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