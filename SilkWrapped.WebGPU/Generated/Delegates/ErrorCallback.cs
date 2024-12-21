using System.Runtime.InteropServices;

namespace SilkWrapped.WebGPU;
public unsafe delegate void ErrorCallback(ErrorType errorType, string? message, void* data);