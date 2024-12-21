using System.Runtime.CompilerServices;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct SurfaceHandle
{
    private readonly Silk.NET.WebGPU.Surface* nativeHandle;
    private SurfaceHandle(Silk.NET.WebGPU.Surface* nativeHandle)
    {
        this.nativeHandle = nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.Surface*(SurfaceHandle handle) => handle.nativeHandle;
    public static implicit operator SurfaceHandle(Silk.NET.WebGPU.Surface* handle) => new SurfaceHandle(handle);
}

public unsafe partial class Surface : System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public SurfaceHandle Handle { get; private set; }

    public Surface(Silk.NET.WebGPU.WebGPU webGPU, SurfaceHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator SurfaceHandle(Surface obj) => obj.Handle;
    public unsafe void Configure(in SurfaceConfiguration config)
    {
        using var m = new MarshalHelper();
        Silk.NET.WebGPU.SurfaceConfiguration __config = default;
        __config.Device = config.Device;
        __config.Format = (Silk.NET.WebGPU.TextureFormat)config.Format;
        __config.Usage = (Silk.NET.WebGPU.TextureUsage)config.Usage;
        if (config.ViewFormats != null)
        {
            __config.ViewFormatCount = (nuint)config.ViewFormats!.Length;
            __config.ViewFormats = m.Pin<TextureFormat, Silk.NET.WebGPU.TextureFormat>(config.ViewFormats);
        }

        __config.AlphaMode = (Silk.NET.WebGPU.CompositeAlphaMode)config.AlphaMode;
        __config.Width = config.Width;
        __config.Height = config.Height;
        __config.PresentMode = (Silk.NET.WebGPU.PresentMode)config.PresentMode;
        WebGPU.SurfaceConfigure(Handle, in __config);
    }

    public unsafe void GetCapabilities(AdapterHandle adapter, ref SurfaceCapabilities capabilities)
    {
        Silk.NET.WebGPU.SurfaceCapabilities __capabilities = default;
        WebGPU.SurfaceGetCapabilities(Handle, adapter, ref __capabilities);
        using var m = new MarshalHelper();
        if (__capabilities.Formats != null)
        {
            capabilities.Formats = new TextureFormat[__capabilities.FormatCount];
            for (int i = 0; i < capabilities.Formats!.Length; i++)
            {
                capabilities.Formats[i] = (TextureFormat)__capabilities.Formats![i];
            }
        }

        if (__capabilities.PresentModes != null)
        {
            capabilities.PresentModes = new PresentMode[__capabilities.PresentModeCount];
            for (int i = 0; i < capabilities.PresentModes!.Length; i++)
            {
                capabilities.PresentModes[i] = (PresentMode)__capabilities.PresentModes![i];
            }
        }

        if (__capabilities.AlphaModes != null)
        {
            capabilities.AlphaModes = new CompositeAlphaMode[__capabilities.AlphaModeCount];
            for (int i = 0; i < capabilities.AlphaModes!.Length; i++)
            {
                capabilities.AlphaModes[i] = (CompositeAlphaMode)__capabilities.AlphaModes![i];
            }
        }
    }

    public unsafe void GetCurrentTexture(ref SurfaceTexture surfaceTexture)
    {
        Silk.NET.WebGPU.SurfaceTexture __surfaceTexture = default;
        WebGPU.SurfaceGetCurrentTexture(Handle, ref __surfaceTexture);
        surfaceTexture = Unsafe.BitCast<Silk.NET.WebGPU.SurfaceTexture, SurfaceTexture>(__surfaceTexture);
    }

    public unsafe TextureFormat GetPreferredFormat(AdapterHandle adapter)
    {
        var result = WebGPU.SurfaceGetPreferredFormat(Handle, adapter);
        return (TextureFormat)result;
    }

    public unsafe void Present()
    {
        WebGPU.SurfacePresent(Handle);
    }

    public unsafe void SetLabel(string label)
    {
        using var m = new MarshalHelper();
        WebGPU.SurfaceSetLabel(Handle, m.RentUtf8Ptr(label));
    }

    public unsafe void Unconfigure()
    {
        WebGPU.SurfaceUnconfigure(Handle);
    }

    public unsafe void Reference()
    {
        WebGPU.SurfaceReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.SurfaceRelease(Handle);
    }

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