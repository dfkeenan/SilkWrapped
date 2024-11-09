namespace SilkWrapped.WebGPU
{
    public unsafe partial class RenderPipelineWrapper : System.IDisposable
    {
        public ApiContainer Api { get; }
        public Silk.NET.WebGPU.RenderPipeline* Handle { get; }

        public RenderPipelineWrapper(ApiContainer api, Silk.NET.WebGPU.RenderPipeline* handle)
        {
            Api = api;
            Handle = handle;
        }

        partial void BindGroupLayoutWrapperCreated(BindGroupLayoutWrapper value);
        public BindGroupLayoutWrapper GetBindGroupLayout(uint groupIndex)
        {
            var result = Api.Core.RenderPipelineGetBindGroupLayout(Handle, groupIndex);
            if (result == null)
                return null;
            var resultWrapper = new BindGroupLayoutWrapper(Api, result);
            BindGroupLayoutWrapperCreated(resultWrapper);
            return resultWrapper;
        }

        public void SetLabel(ref readonly byte label)
        {
            Api.Core.RenderPipelineSetLabel(Handle, in label);
        }

        public void SetLabel(string label)
        {
            Api.Core.RenderPipelineSetLabel(Handle, label);
        }

        public void Reference()
        {
            Api.Core.RenderPipelineReference(Handle);
        }

        public static implicit operator Silk.NET.WebGPU.RenderPipeline*(RenderPipelineWrapper renderPipelineWrapper) => renderPipelineWrapper.Handle;
        public void Dispose()
        {
            if (Handle == default)
                return;
            Disposing();
            Api.Core.RenderPipelineRelease(Handle);
            Disposed();
        }

        partial void Disposing();
        partial void Disposed();
    }
}