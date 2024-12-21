using System;
using Silk.NET.Core.Native;

namespace SilkWrapped.WebGPU;
public readonly struct PfnCreateComputePipelineAsyncCallback : IDisposable
{
    private readonly Silk.NET.WebGPU.PfnCreateComputePipelineAsyncCallback callback;
    public unsafe PfnCreateComputePipelineAsyncCallback(CreateComputePipelineAsyncCallback proc)
    {
        callback = new((status, computePipeline, message, data) =>
        {
            proc((CreatePipelineAsyncStatus)status, computePipeline, SilkMarshal.PtrToString((nint)message, NativeStringEncoding.UTF8), data);
        });
    }

    public static PfnCreateComputePipelineAsyncCallback From(CreateComputePipelineAsyncCallback proc)
    {
        return new PfnCreateComputePipelineAsyncCallback(proc);
    }

    public unsafe void Dispose() => callback.Dispose();
    public static implicit operator Silk.NET.WebGPU.PfnCreateComputePipelineAsyncCallback(PfnCreateComputePipelineAsyncCallback callback) => callback.callback;
}