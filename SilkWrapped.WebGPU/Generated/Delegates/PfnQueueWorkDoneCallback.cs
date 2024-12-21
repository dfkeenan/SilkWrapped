namespace SilkWrapped.WebGPU;
public readonly struct PfnQueueWorkDoneCallback : IDisposable
{
    private readonly Silk.NET.WebGPU.PfnQueueWorkDoneCallback callback;
    public unsafe PfnQueueWorkDoneCallback(QueueWorkDoneCallback proc)
    {
        callback = new((status, data) =>
        {
            proc((QueueWorkDoneStatus)status, data);
        });
    }

    public static PfnQueueWorkDoneCallback From(QueueWorkDoneCallback proc)
    {
        return new PfnQueueWorkDoneCallback(proc);
    }

    public unsafe void Dispose() => callback.Dispose();
    public static implicit operator Silk.NET.WebGPU.PfnQueueWorkDoneCallback(PfnQueueWorkDoneCallback callback) => callback.callback;
}