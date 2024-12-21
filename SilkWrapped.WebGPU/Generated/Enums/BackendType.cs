using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public enum BackendType
{
    Undefined = 0,
    Null = 1,
    WebGpu = 2,
    D3D11 = 3,
    D3D12 = 4,
    Metal = 5,
    Vulkan = 6,
    OpenGL = 7,
    OpenGles = 8,
    Force32 = int.MaxValue
}