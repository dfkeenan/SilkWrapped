using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using Silk.NET.Core;
using Silk.NET.Core.Attributes;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct TextureHandle
{
    private readonly Silk.NET.WebGPU.Texture* nativeHandle;
    private TextureHandle(Silk.NET.WebGPU.Texture* nativeHandle)
    {
        this.nativeHandle = nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.Texture*(TextureHandle handle) => handle.nativeHandle;
    public static implicit operator TextureHandle(Silk.NET.WebGPU.Texture* handle) => new TextureHandle(handle);
}

public unsafe partial class Texture : System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public TextureHandle Handle { get; private set; }

    public Texture(Silk.NET.WebGPU.WebGPU webGPU, TextureHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator TextureHandle(Texture obj) => obj.Handle;
    public unsafe TextureView CreateView(in TextureViewDescriptor descriptor)
    {
        using var m = new MarshalHelper();
        Silk.NET.WebGPU.TextureViewDescriptor __descriptor = default;
        __descriptor.Label = m.RentUtf8Ptr(descriptor.Label);
        __descriptor.Format = (Silk.NET.WebGPU.TextureFormat)descriptor.Format;
        __descriptor.Dimension = (Silk.NET.WebGPU.TextureViewDimension)descriptor.Dimension;
        __descriptor.BaseMipLevel = descriptor.BaseMipLevel;
        __descriptor.MipLevelCount = descriptor.MipLevelCount;
        __descriptor.BaseArrayLayer = descriptor.BaseArrayLayer;
        __descriptor.ArrayLayerCount = descriptor.ArrayLayerCount;
        __descriptor.Aspect = (Silk.NET.WebGPU.TextureAspect)descriptor.Aspect;
        var result = WebGPU.TextureCreateView(Handle, in __descriptor);
        return new TextureView(WebGPU, result);
    }

    public unsafe void Destroy()
    {
        WebGPU.TextureDestroy(Handle);
    }

    public unsafe uint GetDepthOrArrayLayers()
    {
        var result = WebGPU.TextureGetDepthOrArrayLayers(Handle);
        return result;
    }

    public unsafe TextureDimension GetDimension()
    {
        var result = WebGPU.TextureGetDimension(Handle);
        return (TextureDimension)result;
    }

    public unsafe TextureFormat GetFormat()
    {
        var result = WebGPU.TextureGetFormat(Handle);
        return (TextureFormat)result;
    }

    public unsafe uint GetHeight()
    {
        var result = WebGPU.TextureGetHeight(Handle);
        return result;
    }

    public unsafe uint GetMipLevelCount()
    {
        var result = WebGPU.TextureGetMipLevelCount(Handle);
        return result;
    }

    public unsafe uint GetSampleCount()
    {
        var result = WebGPU.TextureGetSampleCount(Handle);
        return result;
    }

    public unsafe TextureUsage GetUsage()
    {
        var result = WebGPU.TextureGetUsage(Handle);
        return (TextureUsage)result;
    }

    public unsafe uint GetWidth()
    {
        var result = WebGPU.TextureGetWidth(Handle);
        return result;
    }

    public unsafe void SetLabel(string label)
    {
        using var m = new MarshalHelper();
        WebGPU.TextureSetLabel(Handle, m.RentUtf8Ptr(label));
    }

    public unsafe void Reference()
    {
        WebGPU.TextureReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.TextureRelease(Handle);
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