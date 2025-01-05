namespace SilkWrapped.WebGPU;
public readonly struct PfnBufferMapCallback : IDisposable
{
    private readonly Silk.NET.WebGPU.PfnBufferMapCallback callback;
    public unsafe PfnBufferMapCallback(BufferMapCallback proc)
    {
        callback = new((status, data) =>
        {
            proc((BufferMapAsyncStatus)status, data);
        });
    }

    public static PfnBufferMapCallback From(BufferMapCallback proc)
    {
        return new PfnBufferMapCallback(proc);
    }

    public unsafe void Dispose() => callback.Dispose();
    public static implicit operator Silk.NET.WebGPU.PfnBufferMapCallback(PfnBufferMapCallback callback) => callback.callback;
}