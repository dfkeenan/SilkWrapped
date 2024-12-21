using System.Runtime.InteropServices;

namespace SilkWrapped.WebGPU;
public unsafe delegate void CreateComputePipelineAsyncCallback(CreatePipelineAsyncStatus status, ComputePipelineHandle computePipeline, string? message, void* data);