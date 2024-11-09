namespace SilkWrapped.WebGPU
{
    public unsafe partial class TextureWrapper : System.IDisposable
    {
        public ApiContainer Api { get; }
        public Silk.NET.WebGPU.Texture* Handle { get; }

        public TextureWrapper(ApiContainer api, Silk.NET.WebGPU.Texture* handle)
        {
            Api = api;
            Handle = handle;
        }

        partial void TextureViewWrapperCreated(TextureViewWrapper value);
        public TextureViewWrapper CreateView(ref readonly Silk.NET.WebGPU.TextureViewDescriptor descriptor)
        {
            var result = Api.Core.TextureCreateView(Handle, in descriptor);
            if (result == null)
                return null;
            var resultWrapper = new TextureViewWrapper(Api, result);
            TextureViewWrapperCreated(resultWrapper);
            return resultWrapper;
        }

        public void Destroy()
        {
            Api.Core.TextureDestroy(Handle);
        }

        public uint GetDepthOrArrayLayers()
        {
            var result = Api.Core.TextureGetDepthOrArrayLayers(Handle);
            return result;
        }

        public Silk.NET.WebGPU.TextureDimension GetDimension()
        {
            var result = Api.Core.TextureGetDimension(Handle);
            return result;
        }

        public Silk.NET.WebGPU.TextureFormat GetFormat()
        {
            var result = Api.Core.TextureGetFormat(Handle);
            return result;
        }

        public uint GetHeight()
        {
            var result = Api.Core.TextureGetHeight(Handle);
            return result;
        }

        public uint GetMipLevelCount()
        {
            var result = Api.Core.TextureGetMipLevelCount(Handle);
            return result;
        }

        public uint GetSampleCount()
        {
            var result = Api.Core.TextureGetSampleCount(Handle);
            return result;
        }

        public Silk.NET.WebGPU.TextureUsage GetUsage()
        {
            var result = Api.Core.TextureGetUsage(Handle);
            return result;
        }

        public uint GetWidth()
        {
            var result = Api.Core.TextureGetWidth(Handle);
            return result;
        }

        public void SetLabel(ref readonly byte label)
        {
            Api.Core.TextureSetLabel(Handle, in label);
        }

        public void SetLabel(string label)
        {
            Api.Core.TextureSetLabel(Handle, label);
        }

        public void Reference()
        {
            Api.Core.TextureReference(Handle);
        }

        public static implicit operator Silk.NET.WebGPU.Texture*(TextureWrapper textureWrapper) => textureWrapper.Handle;
        public void Dispose()
        {
            if (Handle == default)
                return;
            Disposing();
            Api.Core.TextureRelease(Handle);
            Disposed();
        }

        partial void Disposing();
        partial void Disposed();
    }
}