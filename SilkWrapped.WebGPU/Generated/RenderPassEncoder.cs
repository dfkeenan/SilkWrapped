using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct RenderPassEncoderHandle : IEquatable<RenderPassEncoderHandle>
{
    private readonly nint nativeHandle;
    private RenderPassEncoderHandle(Silk.NET.WebGPU.RenderPassEncoder* nativeHandle)
    {
        this.nativeHandle = (nint)nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.RenderPassEncoder*(RenderPassEncoderHandle handle) => (Silk.NET.WebGPU.RenderPassEncoder*)handle.nativeHandle;
    public static implicit operator RenderPassEncoderHandle(Silk.NET.WebGPU.RenderPassEncoder* handle) => new RenderPassEncoderHandle(handle);
    public static bool operator ==(RenderPassEncoderHandle handle, RenderPassEncoderHandle other) => handle.nativeHandle == other.nativeHandle;
    public static bool operator !=(RenderPassEncoderHandle handle, RenderPassEncoderHandle other) => handle.nativeHandle != other.nativeHandle;
    public bool Equals(RenderPassEncoderHandle other) => this == other;
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not RenderPassEncoderHandle other)
            return false;
        return this == other;
    }

    public override int GetHashCode() => (int)nativeHandle;
}

public unsafe partial class RenderPassEncoder : IEquatable<RenderPassEncoder>, System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public RenderPassEncoderHandle Handle { get; private set; }

    public RenderPassEncoder(Silk.NET.WebGPU.WebGPU webGPU, RenderPassEncoderHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator RenderPassEncoderHandle(RenderPassEncoder obj) => obj.Handle;
    public unsafe void BeginOcclusionQuery(uint queryIndex)
    {
        WebGPU.RenderPassEncoderBeginOcclusionQuery(Handle, queryIndex);
    }

    public unsafe void Draw(uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance)
    {
        WebGPU.RenderPassEncoderDraw(Handle, vertexCount, instanceCount, firstVertex, firstInstance);
    }

    public unsafe void DrawIndexed(uint indexCount, uint instanceCount, uint firstIndex, int baseVertex, uint firstInstance)
    {
        WebGPU.RenderPassEncoderDrawIndexed(Handle, indexCount, instanceCount, firstIndex, baseVertex, firstInstance);
    }

    public unsafe void DrawIndexedIndirect(BufferHandle indirectBuffer, ulong indirectOffset)
    {
        WebGPU.RenderPassEncoderDrawIndexedIndirect(Handle, indirectBuffer, indirectOffset);
    }

    public unsafe void DrawIndirect(BufferHandle indirectBuffer, ulong indirectOffset)
    {
        WebGPU.RenderPassEncoderDrawIndirect(Handle, indirectBuffer, indirectOffset);
    }

    public unsafe void End()
    {
        WebGPU.RenderPassEncoderEnd(Handle);
    }

    public unsafe void EndOcclusionQuery()
    {
        WebGPU.RenderPassEncoderEndOcclusionQuery(Handle);
    }

    public unsafe void ExecuteBundles(params ReadOnlySpan<RenderBundleHandle> bundles)
    {
        fixed (RenderBundleHandle* bundlesPtr = bundles)
        {
            WebGPU.RenderPassEncoderExecuteBundles(Handle, (nuint)bundles.Length, (Silk.NET.WebGPU.RenderBundle**)bundlesPtr);
        }
    }

    public unsafe void InsertDebugMarker(string markerLabel)
    {
        using var m = new MarshalHelper();
        WebGPU.RenderPassEncoderInsertDebugMarker(Handle, m.RentUtf8Ptr(markerLabel));
    }

    public unsafe void PopDebugGroup()
    {
        WebGPU.RenderPassEncoderPopDebugGroup(Handle);
    }

    public unsafe void PushDebugGroup(string groupLabel)
    {
        using var m = new MarshalHelper();
        WebGPU.RenderPassEncoderPushDebugGroup(Handle, m.RentUtf8Ptr(groupLabel));
    }

    public unsafe void SetBindGroup(uint groupIndex, BindGroupHandle group, params ReadOnlySpan<uint> dynamicOffsets)
    {
        fixed (UInt32* dynamicOffsetsPtr = dynamicOffsets)
        {
            WebGPU.RenderPassEncoderSetBindGroup(Handle, groupIndex, group, (nuint)dynamicOffsets.Length, dynamicOffsetsPtr);
        }
    }

    public unsafe void SetBlendConstant(in Color color)
    {
        Silk.NET.WebGPU.Color __color = default;
        __color = Unsafe.BitCast<Color, Silk.NET.WebGPU.Color>(color);
        WebGPU.RenderPassEncoderSetBlendConstant(Handle, in __color);
    }

    public unsafe void SetIndexBuffer(BufferHandle buffer, IndexFormat format, ulong offset, ulong size)
    {
        WebGPU.RenderPassEncoderSetIndexBuffer(Handle, buffer, (Silk.NET.WebGPU.IndexFormat)format, offset, size);
    }

    public unsafe void SetLabel(string label)
    {
        using var m = new MarshalHelper();
        WebGPU.RenderPassEncoderSetLabel(Handle, m.RentUtf8Ptr(label));
    }

    public unsafe void SetPipeline(RenderPipelineHandle pipeline)
    {
        WebGPU.RenderPassEncoderSetPipeline(Handle, pipeline);
    }

    public unsafe void SetScissorRect(uint x, uint y, uint width, uint height)
    {
        WebGPU.RenderPassEncoderSetScissorRect(Handle, x, y, width, height);
    }

    public unsafe void SetStencilReference(uint reference)
    {
        WebGPU.RenderPassEncoderSetStencilReference(Handle, reference);
    }

    public unsafe void SetVertexBuffer(uint slot, BufferHandle buffer, ulong offset, ulong size)
    {
        WebGPU.RenderPassEncoderSetVertexBuffer(Handle, slot, buffer, offset, size);
    }

    public unsafe void SetViewport(float x, float y, float width, float height, float minDepth, float maxDepth)
    {
        WebGPU.RenderPassEncoderSetViewport(Handle, x, y, width, height, minDepth, maxDepth);
    }

    public unsafe void Reference()
    {
        WebGPU.RenderPassEncoderReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.RenderPassEncoderRelease(Handle);
    }

    public bool Equals([NotNullWhen(true)] RenderPassEncoder? other)
    {
        if (other is null)
            return false;
        return Handle == other.Handle;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not RenderPassEncoder other)
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