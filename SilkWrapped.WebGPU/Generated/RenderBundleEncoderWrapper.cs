namespace SilkWrapped.WebGPU
{
    public unsafe partial class RenderBundleEncoderWrapper : System.IDisposable
    {
        public ApiContainer Api { get; }
        public Silk.NET.WebGPU.RenderBundleEncoder* Handle { get; }

        public RenderBundleEncoderWrapper(ApiContainer api, Silk.NET.WebGPU.RenderBundleEncoder* handle)
        {
            Api = api;
            Handle = handle;
        }

        public void Draw(uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance)
        {
            Api.Core.RenderBundleEncoderDraw(Handle, vertexCount, instanceCount, firstVertex, firstInstance);
        }

        public void DrawIndexed(uint indexCount, uint instanceCount, uint firstIndex, int baseVertex, uint firstInstance)
        {
            Api.Core.RenderBundleEncoderDrawIndexed(Handle, indexCount, instanceCount, firstIndex, baseVertex, firstInstance);
        }

        public void DrawIndexedIndirect(BufferWrapper indirectBuffer, ulong indirectOffset)
        {
            var indirectBufferRef = indirectBuffer.Handle;
            Api.Core.RenderBundleEncoderDrawIndexedIndirect(Handle, indirectBufferRef, indirectOffset);
        }

        public void DrawIndirect(BufferWrapper indirectBuffer, ulong indirectOffset)
        {
            var indirectBufferRef = indirectBuffer.Handle;
            Api.Core.RenderBundleEncoderDrawIndirect(Handle, indirectBufferRef, indirectOffset);
        }

        partial void RenderBundleWrapperCreated(RenderBundleWrapper value);
        public RenderBundleWrapper Finish(ref readonly Silk.NET.WebGPU.RenderBundleDescriptor descriptor)
        {
            var result = Api.Core.RenderBundleEncoderFinish(Handle, in descriptor);
            if (result == null)
                return null;
            var resultWrapper = new RenderBundleWrapper(Api, result);
            RenderBundleWrapperCreated(resultWrapper);
            return resultWrapper;
        }

        public RenderBundleWrapper Finish()
        {
            Silk.NET.WebGPU.RenderBundleDescriptor descriptor = new Silk.NET.WebGPU.RenderBundleDescriptor();
            var result = Api.Core.RenderBundleEncoderFinish(Handle, in descriptor);
            if (result == null)
                return null;
            var resultWrapper = new RenderBundleWrapper(Api, result);
            RenderBundleWrapperCreated(resultWrapper);
            return resultWrapper;
        }

        public void InsertDebugMarker(ref readonly byte markerLabel)
        {
            Api.Core.RenderBundleEncoderInsertDebugMarker(Handle, in markerLabel);
        }

        public void InsertDebugMarker(string markerLabel)
        {
            Api.Core.RenderBundleEncoderInsertDebugMarker(Handle, markerLabel);
        }

        public void PopDebugGroup()
        {
            Api.Core.RenderBundleEncoderPopDebugGroup(Handle);
        }

        public void PushDebugGroup(ref readonly byte groupLabel)
        {
            Api.Core.RenderBundleEncoderPushDebugGroup(Handle, in groupLabel);
        }

        public void PushDebugGroup(string groupLabel)
        {
            Api.Core.RenderBundleEncoderPushDebugGroup(Handle, groupLabel);
        }

        public void SetBindGroup(uint groupIndex, BindGroupWrapper group, nuint dynamicOffsetCount, ref readonly uint dynamicOffsets)
        {
            var groupRef = group.Handle;
            Api.Core.RenderBundleEncoderSetBindGroup(Handle, groupIndex, groupRef, dynamicOffsetCount, in dynamicOffsets);
        }

        public void SetIndexBuffer(BufferWrapper buffer, Silk.NET.WebGPU.IndexFormat format, ulong offset, ulong size)
        {
            var bufferRef = buffer.Handle;
            Api.Core.RenderBundleEncoderSetIndexBuffer(Handle, bufferRef, format, offset, size);
        }

        public void SetLabel(ref readonly byte label)
        {
            Api.Core.RenderBundleEncoderSetLabel(Handle, in label);
        }

        public void SetLabel(string label)
        {
            Api.Core.RenderBundleEncoderSetLabel(Handle, label);
        }

        public void SetPipeline(RenderPipelineWrapper pipeline)
        {
            var pipelineRef = pipeline.Handle;
            Api.Core.RenderBundleEncoderSetPipeline(Handle, pipelineRef);
        }

        public void SetVertexBuffer(uint slot, BufferWrapper buffer, ulong offset, ulong size)
        {
            var bufferRef = buffer.Handle;
            Api.Core.RenderBundleEncoderSetVertexBuffer(Handle, slot, bufferRef, offset, size);
        }

        public void Reference()
        {
            Api.Core.RenderBundleEncoderReference(Handle);
        }

        public static implicit operator Silk.NET.WebGPU.RenderBundleEncoder*(RenderBundleEncoderWrapper renderBundleEncoderWrapper) => renderBundleEncoderWrapper.Handle;
        public void Dispose()
        {
            if (Handle == default)
                return;
            Disposing();
            Api.Core.RenderBundleEncoderRelease(Handle);
            Disposed();
        }

        partial void Disposing();
        partial void Disposed();
    }
}