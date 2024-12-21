namespace SilkWrapped.WebGPU;
public readonly struct PfnDeviceLostCallback : IDisposable
{
    private readonly Silk.NET.WebGPU.PfnDeviceLostCallback callback;
    public unsafe PfnDeviceLostCallback(DeviceLostCallback proc)
    {
        callback = new((deviceLostReason, message, data) =>
        {
            proc((DeviceLostReason)deviceLostReason, SilkMarshal.PtrToString((nint)message, NativeStringEncoding.UTF8), data);
        });
    }

    public static PfnDeviceLostCallback From(DeviceLostCallback proc)
    {
        return new PfnDeviceLostCallback(proc);
    }

    public unsafe void Dispose() => callback.Dispose();
    public static implicit operator Silk.NET.WebGPU.PfnDeviceLostCallback(PfnDeviceLostCallback callback) => callback.callback;
}