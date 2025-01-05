namespace SilkWrapped.WebGPU;
public unsafe partial class Queue
{
    public unsafe void WriteBuffer<T0>(BufferHandle buffer, ulong bufferOffset, params ReadOnlySpan<T0> data)
        where T0 : unmanaged
    {
        var size = (nuint)(sizeof(T0) * data.Length);
        WriteBuffer(buffer, bufferOffset, data.GetPinnableReference(), size);
    }

    public unsafe void WriteBuffer<T0>(BufferHandle buffer, params ReadOnlySpan<T0> data)
        where T0 : unmanaged
    {
        WriteBuffer(buffer, 0, data);
    }

    public unsafe void WriteTexture<T0>(in ImageCopyTexture destination, ReadOnlySpan<T0> data, in TextureDataLayout dataLayout, in Extent3D writeSize)
       where T0 : unmanaged
    {
        var size = (nuint)(sizeof(T0) * data.Length);
        WriteTexture(in destination, data.GetPinnableReference(), size, dataLayout, writeSize);
    }
}
