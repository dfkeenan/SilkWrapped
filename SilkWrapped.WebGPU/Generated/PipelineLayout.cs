using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using Silk.NET.Core;
using Silk.NET.Core.Attributes;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct PipelineLayoutHandle
{
    private readonly Silk.NET.WebGPU.PipelineLayout* nativeHandle;
    private PipelineLayoutHandle(Silk.NET.WebGPU.PipelineLayout* nativeHandle)
    {
        this.nativeHandle = nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.PipelineLayout*(PipelineLayoutHandle handle) => handle.nativeHandle;
    public static implicit operator PipelineLayoutHandle(Silk.NET.WebGPU.PipelineLayout* handle) => new PipelineLayoutHandle(handle);
}

public unsafe partial class PipelineLayout : System.IDisposable
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