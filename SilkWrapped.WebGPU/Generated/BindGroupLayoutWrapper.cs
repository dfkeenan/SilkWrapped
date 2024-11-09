namespace SilkWrapped.WebGPU
{
    public unsafe partial class BindGroupLayoutWrapper : System.IDisposable
    {
        public ApiContainer Api { get; }
        public Silk.NET.WebGPU.BindGroupLayout* Handle { get; }

        public BindGroupLayoutWrapper(ApiContainer api, Silk.NET.WebGPU.BindGroupLayout* handle)
        {
            Api = api;
            Handle = handle;
        }

        public void SetLabel(ref readonly byte label)
        {
            Api.Core.BindGroupLayoutSetLabel(Handle, in label);
        }

        public void SetLabel(string label)
        {
            Api.Core.BindGroupLayoutSetLabel(Handle, label);
        }

        public void Reference()
        {
            Api.Core.BindGroupLayoutReference(Handle);
        }

        public static implicit operator Silk.NET.WebGPU.BindGroupLayout*(BindGroupLayoutWrapper bindGroupLayoutWrapper) => bindGroupLayoutWrapper.Handle;
        public void Dispose()
        {
            if (Handle == default)
                return;
            Disposing();
            Api.Core.BindGroupLayoutRelease(Handle);
            Disposed();
        }

        partial void Disposing();
        partial void Disposed();
    }
}