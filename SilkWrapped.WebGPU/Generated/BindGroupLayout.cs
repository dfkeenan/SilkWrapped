using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using Silk.NET.Core;
using Silk.NET.Core.Attributes;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct BindGroupLayoutHandle
{
    private readonly Silk.NET.WebGPU.BindGroupLayout* nativeHandle;
    private BindGroupLayoutHandle(Silk.NET.WebGPU.BindGroupLayout* nativeHandle)
    {
        this.nativeHandle = nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.BindGroupLayout*(BindGroupLayoutHandle handle) => handle.nativeHandle;
    public static implicit operator BindGroupLayoutHandle(Silk.NET.WebGPU.BindGroupLayout* handle) => new BindGroupLayoutHandle(handle);
}

public unsafe partial class BindGroupLayout : System.IDisposable
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