namespace SilkWrapped.WebGPU
{
    public unsafe partial class RenderPassEncoderWrapper : System.IDisposable
    {
        public ApiContainer Api { get; }
        public Silk.NET.WebGPU.RenderPassEncoder* Handle { get; }

        public RenderPassEncoderWrapper(ApiContainer api, Silk.NET.WebGPU.RenderPassEncoder* handle)
        {
            Api = api;
            Handle = handle;
        }

        public void BeginOcclusionQuery(uint queryIndex)
        {
            Api.Core.RenderPassEncoderBeginOcclusionQuery(Handle, queryIndex);
        }

        public void Draw(uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance)
        {
            Api.Core.RenderPassEncoderDraw(Handle, vertexCount, instanceCount, firstVertex, firstInstance);
        }

        public void DrawIndexed(uint indexCount, uint instanceCount, uint firstIndex, int baseVertex, uint firstInstance)
        {
            Api.Core.RenderPassEncoderDrawIndexed(Handle, indexCount, instanceCount, firstIndex, baseVertex, firstInstance);
        }

        public void DrawIndexedIndirect(BufferWrapper indirectBuffer, ulong indirectOffset)
        {
            var indirectBufferRef = indirectBuffer.Handle;
            Api.Core.RenderPassEncoderDrawIndexedIndirect(Handle, indirectBufferRef, indirectOffset);
        }

        public void DrawIndirect(BufferWrapper indirectBuffer, ulong indirectOffset)
        {
            var indirectBufferRef = indirectBuffer.Handle;
            Api.Core.RenderPassEncoderDrawIndirect(Handle, indirectBufferRef, indirectOffset);
        }

        public void End()
        {
            Api.Core.RenderPassEncoderEnd(Handle);
        }

        public void EndOcclusionQuery()
        {
            Api.Core.RenderPassEncoderEndOcclusionQuery(Handle);
        }

        public void ExecuteBundles(nuint bundleCount, RenderBundleWrapper bundles)
        {
            var bundlesRef = bundles.Handle;
            Api.Core.RenderPassEncoderExecuteBundles(Handle, bundleCount, ref bundlesRef);
        }

        public void InsertDebugMarker(ref readonly byte markerLabel)
        {
            Api.Core.RenderPassEncoderInsertDebugMarker(Handle, in markerLabel);
        }

        public void InsertDebugMarker(string markerLabel)
        {
            Api.Core.RenderPassEncoderInsertDebugMarker(Handle, markerLabel);
        }

        public void PopDebugGroup()
        {
            Api.Core.RenderPassEncoderPopDebugGroup(Handle);
        }

        public void PushDebugGroup(ref readonly byte groupLabel)
        {
            Api.Core.RenderPassEncoderPushDebugGroup(Handle, in groupLabel);
        }

        public void PushDebugGroup(string groupLabel)
        {
            Api.Core.RenderPassEncoderPushDebugGroup(Handle, groupLabel);
        }

        public void SetBindGroup(uint groupIndex, BindGroupWrapper group, nuint dynamicOffsetCount, ref readonly uint dynamicOffsets)
        {
            var groupRef = group.Handle;
            Api.Core.RenderPassEncoderSetBindGroup(Handle, groupIndex, groupRef, dynamicOffsetCount, in dynamicOffsets);
        }

        public void SetBlendConstant(ref readonly Silk.NET.WebGPU.Color color)
        {
            Api.Core.RenderPassEncoderSetBlendConstant(Handle, in color);
        }

        public void SetIndexBuffer(BufferWrapper buffer, Silk.NET.WebGPU.IndexFormat format, ulong offset, ulong size)
        {
            var bufferRef = buffer.Handle;
            Api.Core.RenderPassEncoderSetIndexBuffer(Handle, bufferRef, format, offset, size);
        }

        public void SetLabel(ref readonly byte label)
        {
            Api.Core.RenderPassEncoderSetLabel(Handle, in label);
        }

        public void SetLabel(string label)
        {
            Api.Core.RenderPassEncoderSetLabel(Handle, label);
        }

        public void SetPipeline(RenderPipelineWrapper pipeline)
        {
            var pipelineRef = pipeline.Handle;
            Api.Core.RenderPassEncoderSetPipeline(Handle, pipelineRef);
        }

        public void SetScissorRect(uint x, uint y, uint width, uint height)
        {
            Api.Core.RenderPassEncoderSetScissorRect(Handle, x, y, width, height);
        }

        public void SetStencilReference(uint reference)
        {
            Api.Core.RenderPassEncoderSetStencilReference(Handle, reference);
        }

        public void SetVertexBuffer(uint slot, BufferWrapper buffer, ulong offset, ulong size)
        {
            var bufferRef = buffer.Handle;
            Api.Core.RenderPassEncoderSetVertexBuffer(Handle, slot, bufferRef, offset, size);
        }

        public void SetViewport(float x, float y, float width, float height, float minDepth, float maxDepth)
        {
            Api.Core.RenderPassEncoderSetViewport(Handle, x, y, width, height, minDepth, maxDepth);
        }

        public void Reference()
        {
            Api.Core.RenderPassEncoderReference(Handle);
        }

        public static implicit operator Silk.NET.WebGPU.RenderPassEncoder*(RenderPassEncoderWrapper renderPassEncoderWrapper) => renderPassEncoderWrapper.Handle;
        public void Dispose()
        {
            if (Handle == default)
                return;
            Disposing();
            Api.Core.RenderPassEncoderRelease(Handle);
            Disposed();
        }

        partial void Disposing();
        partial void Disposed();
    }
}