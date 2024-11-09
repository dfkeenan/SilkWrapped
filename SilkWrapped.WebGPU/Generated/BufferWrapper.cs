namespace SilkWrapped.WebGPU
{
    public unsafe partial class BufferWrapper : System.IDisposable
    {
        public ApiContainer Api { get; }
        public Silk.NET.WebGPU.Buffer* Handle { get; }

        public BufferWrapper(ApiContainer api, Silk.NET.WebGPU.Buffer* handle)
        {
            Api = api;
            Handle = handle;
        }

        public void Destroy()
        {
            Api.Core.BufferDestroy(Handle);
        }

        public void* GetConstMappedRange(nuint offset, nuint size)
        {
            var result = Api.Core.BufferGetConstMappedRange(Handle, offset, size);
            return result;
        }

        public Silk.NET.WebGPU.BufferMapState GetMapState()
        {
            var result = Api.Core.BufferGetMapState(Handle);
            return result;
        }

        public void* GetMappedRange(nuint offset, nuint size)
        {
            var result = Api.Core.BufferGetMappedRange(Handle, offset, size);
            return result;
        }

        public ulong GetSize()
        {
            var result = Api.Core.BufferGetSize(Handle);
            return result;
        }

        public Silk.NET.WebGPU.BufferUsage GetUsage()
        {
            var result = Api.Core.BufferGetUsage(Handle);
            return result;
        }

        public void MapAsync<T0>(Silk.NET.WebGPU.MapMode mode, nuint offset, nuint size, BufferMapCallback callback, ref T0 userdata)
            where T0 : unmanaged
        {
            Silk.NET.WebGPU.PfnBufferMapCallback callbackPfn = new Silk.NET.WebGPU.PfnBufferMapCallback((arg0, arg1) =>
            {
                callback(arg0, arg1);
            });
            Api.Core.BufferMapAsync(Handle, mode, offset, size, callbackPfn, ref userdata);
        }

        public void SetLabel(ref readonly byte label)
        {
            Api.Core.BufferSetLabel(Handle, in label);
        }

        public void SetLabel(string label)
        {
            Api.Core.BufferSetLabel(Handle, label);
        }

        public void Unmap()
        {
            Api.Core.BufferUnmap(Handle);
        }

        public void Reference()
        {
            Api.Core.BufferReference(Handle);
        }

        public static implicit operator Silk.NET.WebGPU.Buffer*(BufferWrapper bufferWrapper) => bufferWrapper.Handle;
        public void Dispose()
        {
            if (Handle == default)
                return;
            Disposing();
            Api.Core.BufferRelease(Handle);
            Disposed();
        }

        partial void Disposing();
        partial void Disposed();
    }
}