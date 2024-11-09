namespace SilkWrapped.WebGPU
{
    public unsafe partial class BindGroupWrapper : System.IDisposable
    {
        public ApiContainer Api { get; }
        public Silk.NET.WebGPU.BindGroup* Handle { get; }

        public BindGroupWrapper(ApiContainer api, Silk.NET.WebGPU.BindGroup* handle)
        {
            Api = api;
            Handle = handle;
        }

        public void SetLabel(ref readonly byte label)
        {
            Api.Core.BindGroupSetLabel(Handle, in label);
        }

        public void SetLabel(string label)
        {
            Api.Core.BindGroupSetLabel(Handle, label);
        }

        public void Reference()
        {
            Api.Core.BindGroupReference(Handle);
        }

        public static implicit operator Silk.NET.WebGPU.BindGroup*(BindGroupWrapper bindGroupWrapper) => bindGroupWrapper.Handle;
        public void Dispose()
        {
            if (Handle == default)
                return;
            Disposing();
            Api.Core.BindGroupRelease(Handle);
            Disposed();
        }

        partial void Disposing();
        partial void Disposed();
    }
}