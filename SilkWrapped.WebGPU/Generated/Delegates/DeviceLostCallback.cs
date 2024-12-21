using System.Runtime.InteropServices;

namespace SilkWrapped.WebGPU;
public unsafe delegate void DeviceLostCallback(DeviceLostReason deviceLostReason, string? message, void* data);