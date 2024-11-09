namespace SilkWrapped.WebGPU
{
    public unsafe partial class SamplerWrapper : System.IDisposable
    {
        public ApiContainer Api { get; }
        public Silk.NET.WebGPU.Sampler* Handle { get; }

        public SamplerWrapper(ApiContainer api, Silk.NET.WebGPU.Sampler* handle)
        {
            Api = api;
            Handle = handle;
        }

        public void SetLabel(ref readonly byte label)
        {
            Api.Core.SamplerSetLabel(Handle, in label);
        }

        public void SetLabel(string label)
        {
            Api.Core.SamplerSetLabel(Handle, label);
        }

        public void Reference()
        {
            Api.Core.SamplerReference(Handle);
        }

        public static implicit operator Silk.NET.WebGPU.Sampler*(SamplerWrapper samplerWrapper) => samplerWrapper.Handle;
        public void Dispose()
        {
            if (Handle == default)
                return;
            Disposing();
            Api.Core.SamplerRelease(Handle);
            Disposed();
        }

        partial void Disposing();
        partial void Disposed();
    }
}