using System.Diagnostics.CodeAnalysis;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct RenderPipelineHandle : IEquatable<RenderPipelineHandle>
{
    private readonly nint nativeHandle;
    private RenderPipelineHandle(Silk.NET.WebGPU.RenderPipeline* nativeHandle)
    {
        this.nativeHandle = (nint)nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.RenderPipeline*(RenderPipelineHandle handle) => (Silk.NET.WebGPU.RenderPipeline*)handle.nativeHandle;
    public static implicit operator RenderPipelineHandle(Silk.NET.WebGPU.RenderPipeline* handle) => new RenderPipelineHandle(handle);
    public static bool operator ==(RenderPipelineHandle handle, RenderPipelineHandle other) => handle.nativeHandle == other.nativeHandle;
    public static bool operator !=(RenderPipelineHandle handle, RenderPipelineHandle other) => handle.nativeHandle != other.nativeHandle;
    public bool Equals(RenderPipelineHandle other) => this == other;
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not RenderPipelineHandle other)
            return false;
        return this == other;
    }

    public override int GetHashCode() => (int)nativeHandle;
}

public unsafe partial class RenderPipeline : IEquatable<RenderPipeline>, System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public RenderPipelineHandle Handle { get; private set; }

    public RenderPipeline(Silk.NET.WebGPU.WebGPU webGPU, RenderPipelineHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator RenderPipelineHandle(RenderPipeline obj) => obj.Handle;
    public unsafe BindGroupLayout GetBindGroupLayout(uint groupIndex)
    {
        var result = WebGPU.RenderPipelineGetBindGroupLayout(Handle, groupIndex);
        return new BindGroupLayout(WebGPU, result);
    }

    public unsafe void SetLabel(string label)
    {
        using var m = new MarshalHelper();
        WebGPU.RenderPipelineSetLabel(Handle, m.RentUtf8Ptr(label));
    }

    public unsafe void Reference()
    {
        WebGPU.RenderPipelineReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.RenderPipelineRelease(Handle);
    }

    public bool Equals([NotNullWhen(true)] RenderPipeline? other)
    {
        if (other is null)
            return false;
        return Handle == other.Handle;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not RenderPipeline other)
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