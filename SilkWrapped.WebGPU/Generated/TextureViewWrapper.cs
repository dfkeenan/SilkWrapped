namespace SilkWrapped.WebGPU
{
    public unsafe partial class TextureViewWrapper : System.IDisposable
    {
        public ApiContainer Api { get; }
        public Silk.NET.WebGPU.TextureView* Handle { get; }

        public TextureViewWrapper(ApiContainer api, Silk.NET.WebGPU.TextureView* handle)
        {
            Api = api;
            Handle = handle;
        }

        public void SetLabel(ref readonly byte label)
        {
            Api.Core.TextureViewSetLabel(Handle, in label);
        }

        public void SetLabel(string label)
        {
            Api.Core.TextureViewSetLabel(Handle, label);
        }

        public void Reference()
        {
            Api.Core.TextureViewReference(Handle);
        }

        public static implicit operator Silk.NET.WebGPU.TextureView*(TextureViewWrapper textureViewWrapper) => textureViewWrapper.Handle;
        public void Dispose()
        {
            if (Handle == default)
                return;
            Disposing();
            Api.Core.TextureViewRelease(Handle);
            Disposed();
        }

        partial void Disposing();
        partial void Disposed();
    }
}