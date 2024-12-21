using System;
using Silk.NET.Core.Native;

namespace SilkWrapped.WebGPU;
public readonly struct PfnAdapterRequestAdapterInfoCallback : IDisposable
{
    private readonly Silk.NET.WebGPU.PfnAdapterRequestAdapterInfoCallback callback;
    public unsafe PfnAdapterRequestAdapterInfoCallback(AdapterRequestAdapterInfoCallback proc)
    {
        callback = new((adapterInfo, data) =>
        {
            AdapterInfo __adapterInfo = default;
            __adapterInfo.Vendor = SilkMarshal.PtrToString((nint)adapterInfo.Vendor, NativeStringEncoding.UTF8);
            __adapterInfo.Architecture = SilkMarshal.PtrToString((nint)adapterInfo.Architecture, NativeStringEncoding.UTF8);
            __adapterInfo.Device = SilkMarshal.PtrToString((nint)adapterInfo.Device, NativeStringEncoding.UTF8);
            __adapterInfo.Description = SilkMarshal.PtrToString((nint)adapterInfo.Description, NativeStringEncoding.UTF8);
            proc(__adapterInfo, data);
        });
    }

    public static PfnAdapterRequestAdapterInfoCallback From(AdapterRequestAdapterInfoCallback proc)
    {
        return new PfnAdapterRequestAdapterInfoCallback(proc);
    }

    public unsafe void Dispose() => callback.Dispose();
    public static implicit operator Silk.NET.WebGPU.PfnAdapterRequestAdapterInfoCallback(PfnAdapterRequestAdapterInfoCallback callback) => callback.callback;
}