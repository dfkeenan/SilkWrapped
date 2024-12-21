using System.Runtime.InteropServices;

namespace SilkWrapped.WebGPU;
public unsafe delegate void RequestDeviceCallback(RequestDeviceStatus status, DeviceHandle device, string? message, void* data);