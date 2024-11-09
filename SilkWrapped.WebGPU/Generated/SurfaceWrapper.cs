namespace SilkWrapped.WebGPU
{
    public unsafe partial class SurfaceWrapper : System.IDisposable
    {
        public ApiContainer Api { get; }
        public Silk.NET.WebGPU.Surface* Handle { get; }

        public SurfaceWrapper(ApiContainer api, Silk.NET.WebGPU.Surface* handle)
        {
            Api = api;
            Handle = handle;
        }

        public void Configure(ref readonly Silk.NET.WebGPU.SurfaceConfiguration config)
        {
            Api.Core.SurfaceConfigure(Handle, in config);
        }

        public void GetCapabilities(AdapterWrapper adapter, ref Silk.NET.WebGPU.SurfaceCapabilities capabilities)
        {
            var adapterRef = adapter.Handle;
            Api.Core.SurfaceGetCapabilities(Handle, adapterRef, ref capabilities);
        }

        public void GetCurrentTexture(ref Silk.NET.WebGPU.SurfaceTexture surfaceTexture)
        {
            Api.Core.SurfaceGetCurrentTexture(Handle, ref surfaceTexture);
        }

        public Silk.NET.WebGPU.TextureFormat GetPreferredFormat(AdapterWrapper adapter)
        {
            var adapterRef = adapter.Handle;
            var result = Api.Core.SurfaceGetPreferredFormat(Handle, adapterRef);
            return result;
        }

        public void Present()
        {
            Api.Core.SurfacePresent(Handle);
        }

        public void SetLabel(ref readonly byte label)
        {
            Api.Core.SurfaceSetLabel(Handle, in label);
        }

        public void SetLabel(string label)
        {
            Api.Core.SurfaceSetLabel(Handle, label);
        }

        public void Unconfigure()
        {
            Api.Core.SurfaceUnconfigure(Handle);
        }

        public void Reference()
        {
            Api.Core.SurfaceReference(Handle);
        }

        public static implicit operator Silk.NET.WebGPU.Surface*(SurfaceWrapper surfaceWrapper) => surfaceWrapper.Handle;
        public void Dispose()
        {
            if (Handle == default)
                return;
            Disposing();
            Api.Core.SurfaceRelease(Handle);
            Disposed();
        }

        partial void Disposing();
        partial void Disposed();
    }
}