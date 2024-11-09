namespace SilkWrapped.WebGPU
{
    public unsafe partial class ComputePipelineWrapper : System.IDisposable
    {
        public ApiContainer Api { get; }
        public Silk.NET.WebGPU.ComputePipeline* Handle { get; }

        public ComputePipelineWrapper(ApiContainer api, Silk.NET.WebGPU.ComputePipeline* handle)
        {
            Api = api;
            Handle = handle;
        }

        partial void BindGroupLayoutWrapperCreated(BindGroupLayoutWrapper value);
        public BindGroupLayoutWrapper GetBindGroupLayout(uint groupIndex)
        {
            var result = Api.Core.ComputePipelineGetBindGroupLayout(Handle, groupIndex);
            if (result == null)
                return null;
            var resultWrapper = new BindGroupLayoutWrapper(Api, result);
            BindGroupLayoutWrapperCreated(resultWrapper);
            return resultWrapper;
        }

        public void SetLabel(ref readonly byte label)
        {
            Api.Core.ComputePipelineSetLabel(Handle, in label);
        }

        public void SetLabel(string label)
        {
            Api.Core.ComputePipelineSetLabel(Handle, label);
        }

        public void Reference()
        {
            Api.Core.ComputePipelineReference(Handle);
        }

        public static implicit operator Silk.NET.WebGPU.ComputePipeline*(ComputePipelineWrapper computePipelineWrapper) => computePipelineWrapper.Handle;
        public void Dispose()
        {
            if (Handle == default)
                return;
            Disposing();
            Api.Core.ComputePipelineRelease(Handle);
            Disposed();
        }

        partial void Disposing();
        partial void Disposed();
    }
}