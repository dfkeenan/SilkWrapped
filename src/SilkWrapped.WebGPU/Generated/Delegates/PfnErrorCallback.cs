namespace SilkWrapped.WebGPU;
public readonly struct PfnErrorCallback : IDisposable
{
    private readonly Silk.NET.WebGPU.PfnErrorCallback callback;
    public unsafe PfnErrorCallback(ErrorCallback proc)
    {
        callback = new((errorType, message, data) =>
        {
            proc((ErrorType)errorType, SilkMarshal.PtrToString((nint)message, NativeStringEncoding.UTF8), data);
        });
    }

    public static PfnErrorCallback From(ErrorCallback proc)
    {
        return new PfnErrorCallback(proc);
    }

    public unsafe void Dispose() => callback.Dispose();
    public static implicit operator Silk.NET.WebGPU.PfnErrorCallback(PfnErrorCallback callback) => callback.callback;
}