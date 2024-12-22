using System.Diagnostics.CodeAnalysis;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct ComputePipelineHandle : IEquatable<ComputePipelineHandle>
{
    private readonly nint nativeHandle;
    private ComputePipelineHandle(Silk.NET.WebGPU.ComputePipeline* nativeHandle)
    {
        this.nativeHandle = (nint)nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.ComputePipeline*(ComputePipelineHandle handle) => (Silk.NET.WebGPU.ComputePipeline*)handle.nativeHandle;
    public static implicit operator ComputePipelineHandle(Silk.NET.WebGPU.ComputePipeline* handle) => new ComputePipelineHandle(handle);
    public static bool operator ==(ComputePipelineHandle handle, ComputePipelineHandle other) => handle.nativeHandle == other.nativeHandle;
    public static bool operator !=(ComputePipelineHandle handle, ComputePipelineHandle other) => handle.nativeHandle != other.nativeHandle;
    public bool Equals(ComputePipelineHandle other) => this == other;
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not ComputePipelineHandle other)
            return false;
        return this == other;
    }

    public override int GetHashCode() => (int)nativeHandle;
}

public unsafe partial class ComputePipeline : IEquatable<ComputePipeline>, System.IDisposable
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

    public bool Equals([NotNullWhen(true)] ComputePipeline? other)
    {
        if (other is null)
            return false;
        return Handle == other.Handle;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not ComputePipeline other)
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