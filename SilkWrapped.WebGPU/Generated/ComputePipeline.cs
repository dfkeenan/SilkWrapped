using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using Silk.NET.Core;
using Silk.NET.Core.Attributes;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct ComputePipelineHandle
{
    private readonly Silk.NET.WebGPU.ComputePipeline* nativeHandle;
    private ComputePipelineHandle(Silk.NET.WebGPU.ComputePipeline* nativeHandle)
    {
        this.nativeHandle = nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.ComputePipeline*(ComputePipelineHandle handle) => handle.nativeHandle;
    public static implicit operator ComputePipelineHandle(Silk.NET.WebGPU.ComputePipeline* handle) => new ComputePipelineHandle(handle);
}

public unsafe partial class ComputePipeline : System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public ComputePipelineHandle Handle { get; private set; }

    public ComputePipeline(Silk.NET.WebGPU.WebGPU webGPU, ComputePipelineHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator ComputePipelineHandle(ComputePipeline obj) => obj.Handle;
    public unsafe BindGroupLayout GetBindGroupLayout(uint groupIndex)
    {
        var result = WebGPU.ComputePipelineGetBindGroupLayout(Handle, groupIndex);
        return new BindGroupLayout(WebGPU, result);
    }

    public unsafe void SetLabel(string label)
    {
        using var m = new MarshalHelper();
        WebGPU.ComputePipelineSetLabel(Handle, m.RentUtf8Ptr(label));
    }

    public unsafe void Reference()
    {
        WebGPU.ComputePipelineReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.ComputePipelineRelease(Handle);
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