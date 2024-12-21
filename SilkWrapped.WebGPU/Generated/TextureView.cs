namespace SilkWrapped.WebGPU;
public unsafe readonly struct TextureViewHandle
{
    private readonly Silk.NET.WebGPU.TextureView* nativeHandle;
    private TextureViewHandle(Silk.NET.WebGPU.TextureView* nativeHandle)
    {
        this.nativeHandle = nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.TextureView*(TextureViewHandle handle) => handle.nativeHandle;
    public static implicit operator TextureViewHandle(Silk.NET.WebGPU.TextureView* handle) => new TextureViewHandle(handle);
}

public unsafe partial class TextureView : System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public TextureViewHandle Handle { get; private set; }

    public TextureView(Silk.NET.WebGPU.WebGPU webGPU, TextureViewHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator TextureViewHandle(TextureView obj) => obj.Handle;
    public unsafe void SetLabel(string label)
    {
        using var m = new MarshalHelper();
        WebGPU.TextureViewSetLabel(Handle, m.RentUtf8Ptr(label));
    }

    public unsafe void Reference()
    {
        WebGPU.TextureViewReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.TextureViewRelease(Handle);
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