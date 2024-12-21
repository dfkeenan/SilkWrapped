using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using Silk.NET.Core;
using Silk.NET.Core.Attributes;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct SamplerHandle
{
    private readonly Silk.NET.WebGPU.Sampler* nativeHandle;
    private SamplerHandle(Silk.NET.WebGPU.Sampler* nativeHandle)
    {
        this.nativeHandle = nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.Sampler*(SamplerHandle handle) => handle.nativeHandle;
    public static implicit operator SamplerHandle(Silk.NET.WebGPU.Sampler* handle) => new SamplerHandle(handle);
}

public unsafe partial class Sampler : System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public SamplerHandle Handle { get; private set; }

    public Sampler(Silk.NET.WebGPU.WebGPU webGPU, SamplerHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator SamplerHandle(Sampler obj) => obj.Handle;
    public unsafe void SetLabel(string label)
    {
        using var m = new MarshalHelper();
        WebGPU.SamplerSetLabel(Handle, m.RentUtf8Ptr(label));
    }

    public unsafe void Reference()
    {
        WebGPU.SamplerReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.SamplerRelease(Handle);
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