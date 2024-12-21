using System;
using Silk.NET.Core.Native;

namespace SilkWrapped.WebGPU;
public readonly struct PfnRequestDeviceCallback : IDisposable
{
    private readonly Silk.NET.WebGPU.PfnRequestDeviceCallback callback;
    public unsafe PfnRequestDeviceCallback(RequestDeviceCallback proc)
    {
        callback = new((status, device, message, data) =>
        {
            proc((RequestDeviceStatus)status, device, SilkMarshal.PtrToString((nint)message, NativeStringEncoding.UTF8), data);
        });
    }

    public static PfnRequestDeviceCallback From(RequestDeviceCallback proc)
    {
        return new PfnRequestDeviceCallback(proc);
    }

    public unsafe void Dispose() => callback.Dispose();
    public static implicit operator Silk.NET.WebGPU.PfnRequestDeviceCallback(PfnRequestDeviceCallback callback) => callback.callback;
}