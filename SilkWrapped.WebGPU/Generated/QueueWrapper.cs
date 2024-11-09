namespace SilkWrapped.WebGPU
{
    public unsafe partial class QueueWrapper : System.IDisposable
    {
        public ApiContainer Api { get; }
        public Silk.NET.WebGPU.Queue* Handle { get; }

        public QueueWrapper(ApiContainer api, Silk.NET.WebGPU.Queue* handle)
        {
            Api = api;
            Handle = handle;
        }

        public void OnSubmittedWorkDone<T0>(QueueWorkDoneCallback callback, ref T0 userdata)
            where T0 : unmanaged
        {
            Silk.NET.WebGPU.PfnQueueWorkDoneCallback callbackPfn = new Silk.NET.WebGPU.PfnQueueWorkDoneCallback((arg0, arg1) =>
            {
                callback(arg0, arg1);
            });
            Api.Core.QueueOnSubmittedWorkDone(Handle, callbackPfn, ref userdata);
        }

        public void SetLabel(ref readonly byte label)
        {
            Api.Core.QueueSetLabel(Handle, in label);
        }

        public void SetLabel(string label)
        {
            Api.Core.QueueSetLabel(Handle, label);
        }

        public void Submit(nuint commandCount, CommandBufferWrapper commands)
        {
            var commandsRef = commands.Handle;
            Api.Core.QueueSubmit(Handle, commandCount, ref commandsRef);
        }

        public void WriteBuffer<T0>(BufferWrapper buffer, ulong bufferOffset, ref readonly T0 data, nuint size)
            where T0 : unmanaged
        {
            var bufferRef = buffer.Handle;
            Api.Core.QueueWriteBuffer(Handle, bufferRef, bufferOffset, in data, size);
        }

        public void WriteTexture<T0>(ref readonly Silk.NET.WebGPU.ImageCopyTexture destination, ref readonly T0 data, nuint dataSize, ref readonly Silk.NET.WebGPU.TextureDataLayout dataLayout, ref readonly Silk.NET.WebGPU.Extent3D writeSize)
            where T0 : unmanaged
        {
            Api.Core.QueueWriteTexture(Handle, in destination, in data, dataSize, in dataLayout, in writeSize);
        }

        public void Reference()
        {
            Api.Core.QueueReference(Handle);
        }

        public static implicit operator Silk.NET.WebGPU.Queue*(QueueWrapper queueWrapper) => queueWrapper.Handle;
        public void Dispose()
        {
            if (Handle == default)
                return;
            Disposing();
            Api.Core.QueueRelease(Handle);
            Disposed();
        }

        partial void Disposing();
        partial void Disposed();
    }
}