using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using Silk.NET.Core;
using Silk.NET.Core.Attributes;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct RenderBundleHandle
{
    private readonly Silk.NET.WebGPU.RenderBundle* nativeHandle;
    private RenderBundleHandle(Silk.NET.WebGPU.RenderBundle* nativeHandle)
    {
        this.nativeHandle = nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.RenderBundle*(RenderBundleHandle handle) => handle.nativeHandle;
    public static implicit operator RenderBundleHandle(Silk.NET.WebGPU.RenderBundle* handle) => new RenderBundleHandle(handle);
}

public unsafe partial class RenderBundle : System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public RenderBundleHandle Handle { get; private set; }

    public RenderBundle(Silk.NET.WebGPU.WebGPU webGPU, RenderBundleHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator RenderBundleHandle(RenderBundle obj) => obj.Handle;
    public unsafe void SetLabel(string label)
    {
        using var m = new MarshalHelper();
        WebGPU.RenderBundleSetLabel(Handle, m.RentUtf8Ptr(label));
    }

    public unsafe void Reference()
    {
        WebGPU.RenderBundleReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.RenderBundleRelease(Handle);
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