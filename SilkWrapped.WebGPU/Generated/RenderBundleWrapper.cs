namespace SilkWrapped.WebGPU
{
    public unsafe partial class RenderBundleWrapper : System.IDisposable
    {
        public ApiContainer Api { get; }
        public Silk.NET.WebGPU.RenderBundle* Handle { get; }

        public RenderBundleWrapper(ApiContainer api, Silk.NET.WebGPU.RenderBundle* handle)
        {
            Api = api;
            Handle = handle;
        }

        public void SetLabel(ref readonly byte label)
        {
            Api.Core.RenderBundleSetLabel(Handle, in label);
        }

        public void SetLabel(string label)
        {
            Api.Core.RenderBundleSetLabel(Handle, label);
        }

        public void Reference()
        {
            Api.Core.RenderBundleReference(Handle);
        }

        public static implicit operator Silk.NET.WebGPU.RenderBundle*(RenderBundleWrapper renderBundleWrapper) => renderBundleWrapper.Handle;
        public void Dispose()
        {
            if (Handle == default)
                return;
            Disposing();
            Api.Core.RenderBundleRelease(Handle);
            Disposed();
        }

        partial void Disposing();
        partial void Disposed();
    }
}