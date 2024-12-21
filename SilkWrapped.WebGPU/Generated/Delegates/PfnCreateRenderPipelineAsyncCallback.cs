using System;
using Silk.NET.Core.Native;

namespace SilkWrapped.WebGPU;
public readonly struct PfnCreateRenderPipelineAsyncCallback : IDisposable
{
    private readonly Silk.NET.WebGPU.PfnCreateRenderPipelineAsyncCallback callback;
    public unsafe PfnCreateRenderPipelineAsyncCallback(CreateRenderPipelineAsyncCallback proc)
    {
        callback = new((status, renderPipeline, message, data) =>
        {
            proc((CreatePipelineAsyncStatus)status, renderPipeline, SilkMarshal.PtrToString((nint)message, NativeStringEncoding.UTF8), data);
        });
    }

    public static PfnCreateRenderPipelineAsyncCallback From(CreateRenderPipelineAsyncCallback proc)
    {
        return new PfnCreateRenderPipelineAsyncCallback(proc);
    }

    public unsafe void Dispose() => callback.Dispose();
    public static implicit operator Silk.NET.WebGPU.PfnCreateRenderPipelineAsyncCallback(PfnCreateRenderPipelineAsyncCallback callback) => callback.callback;
}