using System;
using Silk.NET.Core.Native;

namespace SilkWrapped.WebGPU;
public readonly struct PfnRequestAdapterCallback : IDisposable
{
    private readonly Silk.NET.WebGPU.PfnRequestAdapterCallback callback;
    public unsafe PfnRequestAdapterCallback(RequestAdapterCallback proc)
    {
        callback = new((status, adapter, message, data) =>
        {
            proc((RequestAdapterStatus)status, adapter, SilkMarshal.PtrToString((nint)message, NativeStringEncoding.UTF8), data);
        });
    }

    public static PfnRequestAdapterCallback From(RequestAdapterCallback proc)
    {
        return new PfnRequestAdapterCallback(proc);
    }

    public unsafe void Dispose() => callback.Dispose();
    public static implicit operator Silk.NET.WebGPU.PfnRequestAdapterCallback(PfnRequestAdapterCallback callback) => callback.callback;
}