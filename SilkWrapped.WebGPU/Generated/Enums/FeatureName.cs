using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public enum FeatureName
{
    Undefined = 0,
    DepthClipControl = 1,
    Depth32floatStencil8 = 2,
    TimestampQuery = 3,
    TextureCompressionBC = 4,
    TextureCompressionEtc2 = 5,
    TextureCompressionAstc = 6,
    IndirectFirstInstance = 7,
    ShaderF16 = 8,
    RG11B10UfloatRenderable = 9,
    Bgra8UnormStorage = 10,
    Float32filterable = 11,
    Force32 = int.MaxValue
}