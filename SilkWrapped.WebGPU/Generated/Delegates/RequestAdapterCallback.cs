using System.Runtime.InteropServices;

namespace SilkWrapped.WebGPU;
public unsafe delegate void RequestAdapterCallback(RequestAdapterStatus status, AdapterHandle adapter, string? message, void* data);