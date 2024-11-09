namespace SilkWrapped.WebGPU
{
    public unsafe partial class PipelineLayoutWrapper : System.IDisposable
    {
        public ApiContainer Api { get; }
        public Silk.NET.WebGPU.PipelineLayout* Handle { get; }

        public PipelineLayoutWrapper(ApiContainer api, Silk.NET.WebGPU.PipelineLayout* handle)
        {
            Api = api;
            Handle = handle;
        }

        public void SetLabel(ref readonly byte label)
        {
            Api.Core.PipelineLayoutSetLabel(Handle, in label);
        }

        public void SetLabel(string label)
        {
            Api.Core.PipelineLayoutSetLabel(Handle, label);
        }

        public void Reference()
        {
            Api.Core.PipelineLayoutReference(Handle);
        }

        public static implicit operator Silk.NET.WebGPU.PipelineLayout*(PipelineLayoutWrapper pipelineLayoutWrapper) => pipelineLayoutWrapper.Handle;
        public void Dispose()
        {
            if (Handle == default)
                return;
            Disposing();
            Api.Core.PipelineLayoutRelease(Handle);
            Disposed();
        }

        partial void Disposing();
        partial void Disposed();
    }
}