using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using Silk.NET.Core;
using Silk.NET.Core.Attributes;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct RenderBundleEncoderHandle
{
    private readonly Silk.NET.WebGPU.RenderBundleEncoder* nativeHandle;
    private RenderBundleEncoderHandle(Silk.NET.WebGPU.RenderBundleEncoder* nativeHandle)
    {
        this.nativeHandle = nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.RenderBundleEncoder*(RenderBundleEncoderHandle handle) => handle.nativeHandle;
    public static implicit operator RenderBundleEncoderHandle(Silk.NET.WebGPU.RenderBundleEncoder* handle) => new RenderBundleEncoderHandle(handle);
}

public unsafe partial class RenderBundleEncoder : System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public RenderBundleEncoderHandle Handle { get; private set; }

    public RenderBundleEncoder(Silk.NET.WebGPU.WebGPU webGPU, RenderBundleEncoderHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator RenderBundleEncoderHandle(RenderBundleEncoder obj) => obj.Handle;
    public unsafe void Draw(uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance)
    {
        WebGPU.RenderBundleEncoderDraw(Handle, vertexCount, instanceCount, firstVertex, firstInstance);
    }

    public unsafe void DrawIndexed(uint indexCount, uint instanceCount, uint firstIndex, int baseVertex, uint firstInstance)
    {
        WebGPU.RenderBundleEncoderDrawIndexed(Handle, indexCount, instanceCount, firstIndex, baseVertex, firstInstance);
    }

    public unsafe void DrawIndexedIndirect(BufferHandle indirectBuffer, ulong indirectOffset)
    {
        WebGPU.RenderBundleEncoderDrawIndexedIndirect(Handle, indirectBuffer, indirectOffset);
    }

    public unsafe void DrawIndirect(BufferHandle indirectBuffer, ulong indirectOffset)
    {
        WebGPU.RenderBundleEncoderDrawIndirect(Handle, indirectBuffer, indirectOffset);
    }

    public unsafe RenderBundle Finish(string? label = null)
    {
        using var m = new MarshalHelper();
        Silk.NET.WebGPU.RenderBundleDescriptor __descriptor = default;
        __descriptor.Label = m.RentUtf8Ptr(label);
        var result = WebGPU.RenderBundleEncoderFinish(Handle, in __descriptor);
        return new RenderBundle(WebGPU, result);
    }

    public unsafe void InsertDebugMarker(string markerLabel)
    {
        using var m = new MarshalHelper();
        WebGPU.RenderBundleEncoderInsertDebugMarker(Handle, m.RentUtf8Ptr(markerLabel));
    }

    public unsafe void PopDebugGroup()
    {
        WebGPU.RenderBundleEncoderPopDebugGroup(Handle);
    }

    public unsafe void PushDebugGroup(string groupLabel)
    {
        using var m = new MarshalHelper();
        WebGPU.RenderBundleEncoderPushDebugGroup(Handle, m.RentUtf8Ptr(groupLabel));
    }

    public unsafe void SetBindGroup(uint groupIndex, BindGroupHandle group, ReadOnlySpan<uint> dynamicOffsets)
    {
        fixed (UInt32* dynamicOffsetsPtr = dynamicOffsets)
        {
            WebGPU.RenderBundleEncoderSetBindGroup(Handle, groupIndex, group, (nuint)dynamicOffsets.Length, dynamicOffsetsPtr);
        }
    }

    public unsafe void SetIndexBuffer(BufferHandle buffer, IndexFormat format, ulong offset, ulong size)
    {
        WebGPU.RenderBundleEncoderSetIndexBuffer(Handle, buffer, (Silk.NET.WebGPU.IndexFormat)format, offset, size);
    }

    public unsafe void SetLabel(string label)
    {
        using var m = new MarshalHelper();
        WebGPU.RenderBundleEncoderSetLabel(Handle, m.RentUtf8Ptr(label));
    }

    public unsafe void SetPipeline(RenderPipelineHandle pipeline)
    {
        WebGPU.RenderBundleEncoderSetPipeline(Handle, pipeline);
    }

    public unsafe void SetVertexBuffer(uint slot, BufferHandle buffer, ulong offset, ulong size)
    {
        WebGPU.RenderBundleEncoderSetVertexBuffer(Handle, slot, buffer, offset, size);
    }

    public unsafe void Reference()
    {
        WebGPU.RenderBundleEncoderReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.RenderBundleEncoderRelease(Handle);
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