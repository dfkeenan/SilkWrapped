using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public enum WGSLFeatureName
{
    Undefined = 0,
    ReadonlyAndReadwriteStorageTextures = 1,
    Packed4x8integerDotProduct = 2,
    UnrestrictedPointerParameters = 3,
    PointerCompositeAccess = 4,
    Force32 = int.MaxValue
}