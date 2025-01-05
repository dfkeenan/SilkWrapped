using System.Diagnostics.CodeAnalysis;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct TextureViewHandle : IEquatable<TextureViewHandle>
{
    private readonly nint nativeHandle;
    private TextureViewHandle(Silk.NET.WebGPU.TextureView* nativeHandle)
    {
        this.nativeHandle = (nint)nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.TextureView*(TextureViewHandle handle) => (Silk.NET.WebGPU.TextureView*)handle.nativeHandle;
    public static implicit operator TextureViewHandle(Silk.NET.WebGPU.TextureView* handle) => new TextureViewHandle(handle);
    public static bool operator ==(TextureViewHandle handle, TextureViewHandle other) => handle.nativeHandle == other.nativeHandle;
    public static bool operator !=(TextureViewHandle handle, TextureViewHandle other) => handle.nativeHandle != other.nativeHandle;
    public bool Equals(TextureViewHandle other) => this == other;
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not TextureViewHandle other)
            return false;
        return this == other;
    }

    public override int GetHashCode() => (int)nativeHandle;
}

public unsafe partial class TextureView : IEquatable<TextureView>, System.IDisposable
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

    public bool Equals([NotNullWhen(true)] TextureView? other)
    {
        if (other is null)
            return false;
        return Handle == other.Handle;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not TextureView other)
            return false;
        return Handle == other.Handle;
    }

    public override int GetHashCode() => Handle.GetHashCode();
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