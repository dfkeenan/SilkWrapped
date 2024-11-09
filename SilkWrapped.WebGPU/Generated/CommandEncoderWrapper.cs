namespace SilkWrapped.WebGPU
{
    public unsafe partial class CommandEncoderWrapper : System.IDisposable
    {
        public ApiContainer Api { get; }
        public Silk.NET.WebGPU.CommandEncoder* Handle { get; }

        public CommandEncoderWrapper(ApiContainer api, Silk.NET.WebGPU.CommandEncoder* handle)
        {
            Api = api;
            Handle = handle;
        }

        partial void ComputePassEncoderWrapperCreated(ComputePassEncoderWrapper value);
        public ComputePassEncoderWrapper BeginComputePass(ref readonly Silk.NET.WebGPU.ComputePassDescriptor descriptor)
        {
            var result = Api.Core.CommandEncoderBeginComputePass(Handle, in descriptor);
            if (result == null)
                return null;
            var resultWrapper = new ComputePassEncoderWrapper(Api, result);
            ComputePassEncoderWrapperCreated(resultWrapper);
            return resultWrapper;
        }

        partial void RenderPassEncoderWrapperCreated(RenderPassEncoderWrapper value);
        public RenderPassEncoderWrapper BeginRenderPass(ref readonly Silk.NET.WebGPU.RenderPassDescriptor descriptor)
        {
            var result = Api.Core.CommandEncoderBeginRenderPass(Handle, in descriptor);
            if (result == null)
                return null;
            var resultWrapper = new RenderPassEncoderWrapper(Api, result);
            RenderPassEncoderWrapperCreated(resultWrapper);
            return resultWrapper;
        }

        public void ClearBuffer(BufferWrapper buffer, ulong offset, ulong size)
        {
            var bufferRef = buffer.Handle;
            Api.Core.CommandEncoderClearBuffer(Handle, bufferRef, offset, size);
        }

        public void CopyBufferToBuffer(BufferWrapper source, ulong sourceOffset, BufferWrapper destination, ulong destinationOffset, ulong size)
        {
            var sourceRef = source.Handle;
            var destinationRef = destination.Handle;
            Api.Core.CommandEncoderCopyBufferToBuffer(Handle, sourceRef, sourceOffset, destinationRef, destinationOffset, size);
        }

        public void CopyBufferToTexture(ref readonly Silk.NET.WebGPU.ImageCopyBuffer source, ref readonly Silk.NET.WebGPU.ImageCopyTexture destination, ref readonly Silk.NET.WebGPU.Extent3D copySize)
        {
            Api.Core.CommandEncoderCopyBufferToTexture(Handle, in source, in destination, in copySize);
        }

        public void CopyTextureToBuffer(ref readonly Silk.NET.WebGPU.ImageCopyTexture source, ref readonly Silk.NET.WebGPU.ImageCopyBuffer destination, ref readonly Silk.NET.WebGPU.Extent3D copySize)
        {
            Api.Core.CommandEncoderCopyTextureToBuffer(Handle, in source, in destination, in copySize);
        }

        public void CopyTextureToTexture(ref readonly Silk.NET.WebGPU.ImageCopyTexture source, ref readonly Silk.NET.WebGPU.ImageCopyTexture destination, ref readonly Silk.NET.WebGPU.Extent3D copySize)
        {
            Api.Core.CommandEncoderCopyTextureToTexture(Handle, in source, in destination, in copySize);
        }

        partial void CommandBufferWrapperCreated(CommandBufferWrapper value);
        public CommandBufferWrapper Finish(ref readonly Silk.NET.WebGPU.CommandBufferDescriptor descriptor)
        {
            var result = Api.Core.CommandEncoderFinish(Handle, in descriptor);
            if (result == null)
                return null;
            var resultWrapper = new CommandBufferWrapper(Api, result);
            CommandBufferWrapperCreated(resultWrapper);
            return resultWrapper;
        }

        public CommandBufferWrapper Finish()
        {
            Silk.NET.WebGPU.CommandBufferDescriptor descriptor = new Silk.NET.WebGPU.CommandBufferDescriptor();
            var result = Api.Core.CommandEncoderFinish(Handle, in descriptor);
            if (result == null)
                return null;
            var resultWrapper = new CommandBufferWrapper(Api, result);
            CommandBufferWrapperCreated(resultWrapper);
            return resultWrapper;
        }

        public void InsertDebugMarker(ref readonly byte markerLabel)
        {
            Api.Core.CommandEncoderInsertDebugMarker(Handle, in markerLabel);
        }

        public void InsertDebugMarker(string markerLabel)
        {
            Api.Core.CommandEncoderInsertDebugMarker(Handle, markerLabel);
        }

        public void PopDebugGroup()
        {
            Api.Core.CommandEncoderPopDebugGroup(Handle);
        }

        public void PushDebugGroup(ref readonly byte groupLabel)
        {
            Api.Core.CommandEncoderPushDebugGroup(Handle, in groupLabel);
        }

        public void PushDebugGroup(string groupLabel)
        {
            Api.Core.CommandEncoderPushDebugGroup(Handle, groupLabel);
        }

        public void ResolveQuerySet(QuerySetWrapper querySet, uint firstQuery, uint queryCount, BufferWrapper destination, ulong destinationOffset)
        {
            var querySetRef = querySet.Handle;
            var destinationRef = destination.Handle;
            Api.Core.CommandEncoderResolveQuerySet(Handle, querySetRef, firstQuery, queryCount, destinationRef, destinationOffset);
        }

        public void SetLabel(ref readonly byte label)
        {
            Api.Core.CommandEncoderSetLabel(Handle, in label);
        }

        public void SetLabel(string label)
        {
            Api.Core.CommandEncoderSetLabel(Handle, label);
        }

        public void WriteTimestamp(QuerySetWrapper querySet, uint queryIndex)
        {
            var querySetRef = querySet.Handle;
            Api.Core.CommandEncoderWriteTimestamp(Handle, querySetRef, queryIndex);
        }

        public void Reference()
        {
            Api.Core.CommandEncoderReference(Handle);
        }

        public static implicit operator Silk.NET.WebGPU.CommandEncoder*(CommandEncoderWrapper commandEncoderWrapper) => commandEncoderWrapper.Handle;
        public void Dispose()
        {
            if (Handle == default)
                return;
            Disposing();
            Api.Core.CommandEncoderRelease(Handle);
            Disposed();
        }

        partial void Disposing();
        partial void Disposed();
    }
}