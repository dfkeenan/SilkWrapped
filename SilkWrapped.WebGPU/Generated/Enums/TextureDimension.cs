using System;
using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public enum TextureDimension
{
    [Obsolete("Deprecated in favour of \"Dimension1D\"")]
    TextureDimension1D = 0,
    [Obsolete("Deprecated in favour of \"Dimension2D\"")]
    TextureDimension2D = 1,
    [Obsolete("Deprecated in favour of \"Dimension3D\"")]
    TextureDimension3D = 2,
    [Obsolete("Deprecated in favour of \"DimensionForce32\"")]
    TextureDimensionForce32 = int.MaxValue,
    Dimension1D = 0,
    Dimension2D = 1,
    Dimension3D = 2,
    DimensionForce32 = int.MaxValue
}