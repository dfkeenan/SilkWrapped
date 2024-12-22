namespace SilkWrapped.WebGPU;
public class Buffer<T> : Buffer
    where T : unmanaged
{
    public Buffer(Buffer buffer)
        : this(buffer.WebGPU, buffer.Handle, buffer.GetSize())
    {

    }

    public Buffer(Silk.NET.WebGPU.WebGPU webGPU, BufferHandle handle, ulong size)
        : base(webGPU, handle)
    {
        Size = size;
    }

    public ulong Size { get; }
}


public unsafe partial class Device
{
    public unsafe Buffer<T> CreateBuffer<T>(BufferUsage usage, ulong length = 1, bool mappedAtCreation = false)
        where T : unmanaged
    {
        var descriptor = new BufferDescriptor
        {
            Size = (ulong)sizeof(T) * length,
            Usage = usage,
            MappedAtCreation = mappedAtCreation
        };

        return new Buffer<T>(CreateBuffer(in descriptor));
    }
}