using System.Runtime.InteropServices;

namespace SilkWrapped.WebGPU;
public unsafe delegate void QueueWorkDoneCallback(QueueWorkDoneStatus status, void* data);