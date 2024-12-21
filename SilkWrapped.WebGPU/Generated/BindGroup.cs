using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using Silk.NET.Core;
using Silk.NET.Core.Attributes;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct BindGroupHandle
{
    private readonly Silk.NET.WebGPU.BindGroup* nativeHandle;
    private BindGroupHandle(Silk.NET.WebGPU.BindGroup* nativeHandle)
    {
        this.nativeHandle = nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.BindGroup*(BindGroupHandle handle) => handle.nativeHandle;
    public static implicit operator BindGroupHandle(Silk.NET.WebGPU.BindGroup* handle) => new BindGroupHandle(handle);
}

public unsafe partial class BindGroup : System.IDisposable
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