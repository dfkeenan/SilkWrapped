using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct CommandEncoderHandle : IEquatable<CommandEncoderHandle>
{
    private readonly nint nativeHandle;
    private CommandEncoderHandle(Silk.NET.WebGPU.CommandEncoder* nativeHandle)
    {
        this.nativeHandle = (nint)nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.CommandEncoder*(CommandEncoderHandle handle) => (Silk.NET.WebGPU.CommandEncoder*)handle.nativeHandle;
    public static implicit operator CommandEncoderHandle(Silk.NET.WebGPU.CommandEncoder* handle) => new CommandEncoderHandle(handle);
    public static bool operator ==(CommandEncoderHandle handle, CommandEncoderHandle other) => handle.nativeHandle == other.nativeHandle;
    public static bool operator !=(CommandEncoderHandle handle, CommandEncoderHandle other) => handle.nativeHandle != other.nativeHandle;
    public bool Equals(CommandEncoderHandle other) => this == other;
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not CommandEncoderHandle other)
            return false;
        return this == other;
    }

    public override int GetHashCode() => (int)nativeHandle;
}

public unsafe partial class CommandEncoder : IEquatable<CommandEncoder>, System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public CommandEncoderHandle Handle { get; private set; }

    public CommandEncoder(Silk.NET.WebGPU.WebGPU webGPU, CommandEncoderHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator CommandEncoderHandle(CommandEncoder obj) => obj.Handle;
    public unsafe ComputePassEncoder BeginComputePass(in ComputePassDescriptor descriptor)
    {
        using var m = new MarshalHelper();
        Silk.NET.WebGPU.ComputePassDescriptor __descriptor = default;
        __descriptor.Label = m.RentUtf8Ptr(descriptor.Label);
        Silk.NET.WebGPU.ComputePassTimestampWrites __descriptor_TimestampWrites = default;
        if (descriptor.TimestampWrites.HasValue)
        {
            __descriptor_TimestampWrites = Unsafe.BitCast<ComputePassTimestampWrites, Silk.NET.WebGPU.ComputePassTimestampWrites>(descriptor.TimestampWrites!.Value);
            __descriptor.TimestampWrites = &__descriptor_TimestampWrites; //PINREF
        }

        var result = WebGPU.CommandEncoderBeginComputePass(Handle, in __descriptor);
        return new ComputePassEncoder(WebGPU, result);
    }

    public unsafe RenderPassEncoder BeginRenderPass(in RenderPassDescriptor descriptor)
    {
        using var m = new MarshalHelper();
        Silk.NET.WebGPU.RenderPassDescriptor __descriptor = default;
        __descriptor.Label = m.RentUtf8Ptr(descriptor.Label);
        if (descriptor.ColorAttachments != null)
        {
            __descriptor.ColorAttachmentCount = (nuint)descriptor.ColorAttachments!.Length;
            __descriptor.ColorAttachments = MarshalHelper.AsPointer(descriptor.ColorAttachments!.Length <= MarshalHelper.MaxStack ? stackalloc Silk.NET.WebGPU.RenderPassColorAttachment[descriptor.ColorAttachments!.Length] : m.RentSpan<Silk.NET.WebGPU.RenderPassColorAttachment>(descriptor.ColorAttachments!.Length));
            for (int i = 0; i < descriptor.ColorAttachments!.Length; i++)
            {
                __descriptor.ColorAttachments[i].View = descriptor.ColorAttachments![i].View;
                __descriptor.ColorAttachments[i].DepthSlice = descriptor.ColorAttachments![i].DepthSlice;
                __descriptor.ColorAttachments[i].ResolveTarget = descriptor.ColorAttachments![i].ResolveTarget;
                __descriptor.ColorAttachments[i].LoadOp = (Silk.NET.WebGPU.LoadOp)descriptor.ColorAttachments![i].LoadOp;
                __descriptor.ColorAttachments[i].StoreOp = (Silk.NET.WebGPU.StoreOp)descriptor.ColorAttachments![i].StoreOp;
                __descriptor.ColorAttachments[i].ClearValue = Unsafe.BitCast<Color, Silk.NET.WebGPU.Color>(descriptor.ColorAttachments![i].ClearValue);
            }
        }

        Silk.NET.WebGPU.RenderPassDepthStencilAttachment __descriptor_DepthStencilAttachment = default;
        if (descriptor.DepthStencilAttachment.HasValue)
        {
            __descriptor_DepthStencilAttachment = Unsafe.BitCast<RenderPassDepthStencilAttachment, Silk.NET.WebGPU.RenderPassDepthStencilAttachment>(descriptor.DepthStencilAttachment!.Value);
            __descriptor.DepthStencilAttachment = &__descriptor_DepthStencilAttachment; //PINREF
        }

        __descriptor.OcclusionQuerySet = descriptor.OcclusionQuerySet;
        Silk.NET.WebGPU.RenderPassTimestampWrites __descriptor_TimestampWrites = default;
        if (descriptor.TimestampWrites.HasValue)
        {
            __descriptor_TimestampWrites = Unsafe.BitCast<RenderPassTimestampWrites, Silk.NET.WebGPU.RenderPassTimestampWrites>(descriptor.TimestampWrites!.Value);
            __descriptor.TimestampWrites = &__descriptor_TimestampWrites; //PINREF
        }

        var result = WebGPU.CommandEncoderBeginRenderPass(Handle, in __descriptor);
        return new RenderPassEncoder(WebGPU, result);
    }

    public unsafe void ClearBuffer(BufferHandle buffer, ulong offset, ulong size)
    {
        WebGPU.CommandEncoderClearBuffer(Handle, buffer, offset, size);
    }

    public unsafe void CopyBufferToBuffer(BufferHandle source, ulong sourceOffset, BufferHandle destination, ulong destinationOffset, ulong size)
    {
        WebGPU.CommandEncoderCopyBufferToBuffer(Handle, source, sourceOffset, destination, destinationOffset, size);
    }

    public unsafe void CopyBufferToTexture(in ImageCopyBuffer source, in ImageCopyTexture destination, in Extent3D copySize)
    {
        Silk.NET.WebGPU.ImageCopyBuffer __source = default;
        __source.Layout.Offset = source.Layout.Offset;
        __source.Layout.BytesPerRow = source.Layout.BytesPerRow;
        __source.Layout.RowsPerImage = source.Layout.RowsPerImage;
        __source.Buffer = source.Buffer;
        Silk.NET.WebGPU.ImageCopyTexture __destination = default;
        __destination.Texture = destination.Texture;
        __destination.MipLevel = destination.MipLevel;
        __destination.Origin = Unsafe.BitCast<Origin3D, Silk.NET.WebGPU.Origin3D>(destination.Origin);
        __destination.Aspect = (Silk.NET.WebGPU.TextureAspect)destination.Aspect;
        Silk.NET.WebGPU.Extent3D __copySize = default;
        __copySize = Unsafe.BitCast<Extent3D, Silk.NET.WebGPU.Extent3D>(copySize);
        WebGPU.CommandEncoderCopyBufferToTexture(Handle, in __source, in __destination, in __copySize);
    }

    public unsafe void CopyTextureToBuffer(in ImageCopyTexture source, in ImageCopyBuffer destination, in Extent3D copySize)
    {
        Silk.NET.WebGPU.ImageCopyTexture __source = default;
        __source.Texture = source.Texture;
        __source.MipLevel = source.MipLevel;
        __source.Origin = Unsafe.BitCast<Origin3D, Silk.NET.WebGPU.Origin3D>(source.Origin);
        __source.Aspect = (Silk.NET.WebGPU.TextureAspect)source.Aspect;
        Silk.NET.WebGPU.ImageCopyBuffer __destination = default;
        __destination.Layout.Offset = destination.Layout.Offset;
        __destination.Layout.BytesPerRow = destination.Layout.BytesPerRow;
        __destination.Layout.RowsPerImage = destination.Layout.RowsPerImage;
        __destination.Buffer = destination.Buffer;
        Silk.NET.WebGPU.Extent3D __copySize = default;
        __copySize = Unsafe.BitCast<Extent3D, Silk.NET.WebGPU.Extent3D>(copySize);
        WebGPU.CommandEncoderCopyTextureToBuffer(Handle, in __source, in __destination, in __copySize);
    }

    public unsafe void CopyTextureToTexture(in ImageCopyTexture source, in ImageCopyTexture destination, in Extent3D copySize)
    {
        Silk.NET.WebGPU.ImageCopyTexture __source = default;
        __source.Texture = source.Texture;
        __source.MipLevel = source.MipLevel;
        __source.Origin = Unsafe.BitCast<Origin3D, Silk.NET.WebGPU.Origin3D>(source.Origin);
        __source.Aspect = (Silk.NET.WebGPU.TextureAspect)source.Aspect;
        Silk.NET.WebGPU.ImageCopyTexture __destination = default;
        __destination.Texture = destination.Texture;
        __destination.MipLevel = destination.MipLevel;
        __destination.Origin = Unsafe.BitCast<Origin3D, Silk.NET.WebGPU.Origin3D>(destination.Origin);
        __destination.Aspect = (Silk.NET.WebGPU.TextureAspect)destination.Aspect;
        Silk.NET.WebGPU.Extent3D __copySize = default;
        __copySize = Unsafe.BitCast<Extent3D, Silk.NET.WebGPU.Extent3D>(copySize);
        WebGPU.CommandEncoderCopyTextureToTexture(Handle, in __source, in __destination, in __copySize);
    }

    public unsafe CommandBuffer Finish(string? label = null)
    {
        using var m = new MarshalHelper();
        Silk.NET.WebGPU.CommandBufferDescriptor __descriptor = default;
        __descriptor.Label = m.RentUtf8Ptr(label);
        var result = WebGPU.CommandEncoderFinish(Handle, in __descriptor);
        return new CommandBuffer(WebGPU, result);
    }

    public unsafe void InsertDebugMarker(string markerLabel)
    {
        using var m = new MarshalHelper();
        WebGPU.CommandEncoderInsertDebugMarker(Handle, m.RentUtf8Ptr(markerLabel));
    }

    public unsafe void PopDebugGroup()
    {
        WebGPU.CommandEncoderPopDebugGroup(Handle);
    }

    public unsafe void PushDebugGroup(string groupLabel)
    {
        using var m = new MarshalHelper();
        WebGPU.CommandEncoderPushDebugGroup(Handle, m.RentUtf8Ptr(groupLabel));
    }

    public unsafe void ResolveQuerySet(QuerySetHandle querySet, uint firstQuery, uint queryCount, BufferHandle destination, ulong destinationOffset)
    {
        WebGPU.CommandEncoderResolveQuerySet(Handle, querySet, firstQuery, queryCount, destination, destinationOffset);
    }

    public unsafe void SetLabel(string label)
    {
        using var m = new MarshalHelper();
        WebGPU.CommandEncoderSetLabel(Handle, m.RentUtf8Ptr(label));
    }

    public unsafe void WriteTimestamp(QuerySetHandle querySet, uint queryIndex)
    {
        WebGPU.CommandEncoderWriteTimestamp(Handle, querySet, queryIndex);
    }

    public unsafe void Reference()
    {
        WebGPU.CommandEncoderReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.CommandEncoderRelease(Handle);
    }

    public bool Equals([NotNullWhen(true)] CommandEncoder? other)
    {
        if (other is null)
            return false;
        return Handle == other.Handle;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not CommandEncoder other)
            return false;
        return Handle == other.Handle;
    }

    public override int GetHashCode() => Handle.GetHashCode();
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