namespace SilkWrapped.WebGPU
{
    public unsafe partial class ComputePassEncoderWrapper : System.IDisposable
    {
        public ApiContainer Api { get; }
        public Silk.NET.WebGPU.ComputePassEncoder* Handle { get; }

        public ComputePassEncoderWrapper(ApiContainer api, Silk.NET.WebGPU.ComputePassEncoder* handle)
        {
            Api = api;
            Handle = handle;
        }

        public void DispatchWorkgroups(uint workgroupCountX, uint workgroupCountY, uint workgroupCountZ)
        {
            Api.Core.ComputePassEncoderDispatchWorkgroups(Handle, workgroupCountX, workgroupCountY, workgroupCountZ);
        }

        public void DispatchWorkgroupsIndirect(BufferWrapper indirectBuffer, ulong indirectOffset)
        {
            var indirectBufferRef = indirectBuffer.Handle;
            Api.Core.ComputePassEncoderDispatchWorkgroupsIndirect(Handle, indirectBufferRef, indirectOffset);
        }

        public void End()
        {
            Api.Core.ComputePassEncoderEnd(Handle);
        }

        public void InsertDebugMarker(ref readonly byte markerLabel)
        {
            Api.Core.ComputePassEncoderInsertDebugMarker(Handle, in markerLabel);
        }

        public void InsertDebugMarker(string markerLabel)
        {
            Api.Core.ComputePassEncoderInsertDebugMarker(Handle, markerLabel);
        }

        public void PopDebugGroup()
        {
            Api.Core.ComputePassEncoderPopDebugGroup(Handle);
        }

        public void PushDebugGroup(ref readonly byte groupLabel)
        {
            Api.Core.ComputePassEncoderPushDebugGroup(Handle, in groupLabel);
        }

        public void PushDebugGroup(string groupLabel)
        {
            Api.Core.ComputePassEncoderPushDebugGroup(Handle, groupLabel);
        }

        public void SetBindGroup(uint groupIndex, BindGroupWrapper group, nuint dynamicOffsetCount, ref readonly uint dynamicOffsets)
        {
            var groupRef = group.Handle;
            Api.Core.ComputePassEncoderSetBindGroup(Handle, groupIndex, groupRef, dynamicOffsetCount, in dynamicOffsets);
        }

        public void SetLabel(ref readonly byte label)
        {
            Api.Core.ComputePassEncoderSetLabel(Handle, in label);
        }

        public void SetLabel(string label)
        {
            Api.Core.ComputePassEncoderSetLabel(Handle, label);
        }

        public void SetPipeline(ComputePipelineWrapper pipeline)
        {
            var pipelineRef = pipeline.Handle;
            Api.Core.ComputePassEncoderSetPipeline(Handle, pipelineRef);
        }

        public void Reference()
        {
            Api.Core.ComputePassEncoderReference(Handle);
        }

        public static implicit operator Silk.NET.WebGPU.ComputePassEncoder*(ComputePassEncoderWrapper computePassEncoderWrapper) => computePassEncoderWrapper.Handle;
        public void Dispose()
        {
            if (Handle == default)
                return;
            Disposing();
            Api.Core.ComputePassEncoderRelease(Handle);
            Disposed();
        }

        partial void Disposing();
        partial void Disposed();
    }
}