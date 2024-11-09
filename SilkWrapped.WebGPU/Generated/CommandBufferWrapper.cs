namespace SilkWrapped.WebGPU
{
    public unsafe partial class CommandBufferWrapper : System.IDisposable
    {
        public ApiContainer Api { get; }
        public Silk.NET.WebGPU.CommandBuffer* Handle { get; }

        public CommandBufferWrapper(ApiContainer api, Silk.NET.WebGPU.CommandBuffer* handle)
        {
            Api = api;
            Handle = handle;
        }

        public void SetLabel(ref readonly byte label)
        {
            Api.Core.CommandBufferSetLabel(Handle, in label);
        }

        public void SetLabel(string label)
        {
            Api.Core.CommandBufferSetLabel(Handle, label);
        }

        public void Reference()
        {
            Api.Core.CommandBufferReference(Handle);
        }

        public static implicit operator Silk.NET.WebGPU.CommandBuffer*(CommandBufferWrapper commandBufferWrapper) => commandBufferWrapper.Handle;
        public void Dispose()
        {
            if (Handle == default)
                return;
            Disposing();
            Api.Core.CommandBufferRelease(Handle);
            Disposed();
        }

        partial void Disposing();
        partial void Disposed();
    }
}