namespace SilkWrapped.WebGPU;
public unsafe partial class Queue
{
    public unsafe void WriteBuffer<T0>(BufferHandle buffer, ulong bufferOffset, params ReadOnlySpan<T0> data)
        where T0 : unmanaged
    {
        fixed (T0* dataPtr = data)
        {
            var size = (nuint)(sizeof(T0) * data.Length);
            WebGPU.QueueWriteBuffer(Handle, buffer, bufferOffset, dataPtr, size);
        }

    }

    public unsafe void WriteBuffer<T0>(BufferHandle buffer, params ReadOnlySpan<T0> data)
        where T0 : unmanaged
    {
        WriteBuffer(buffer, 0, data);
    }
}
