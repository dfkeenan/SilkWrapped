using System;
using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public enum TextureViewDimension
{
    [Obsolete("Deprecated in favour of \"DimensionUndefined\"")]
    TextureViewDimensionUndefined = 0,
    [Obsolete("Deprecated in favour of \"Dimension1D\"")]
    TextureViewDimension1D = 1,
    [Obsolete("Deprecated in favour of \"Dimension2D\"")]
    TextureViewDimension2D = 2,
    [Obsolete("Deprecated in favour of \"Dimension2DArray\"")]
    TextureViewDimension2DArray = 3,
    [Obsolete("Deprecated in favour of \"DimensionCube\"")]
    TextureViewDimensionCube = 4,
    [Obsolete("Deprecated in favour of \"DimensionCubeArray\"")]
    TextureViewDimensionCubeArray = 5,
    [Obsolete("Deprecated in favour of \"Dimension3D\"")]
    TextureViewDimension3D = 6,
    [Obsolete("Deprecated in favour of \"DimensionForce32\"")]
    TextureViewDimensionForce32 = int.MaxValue,
    DimensionUndefined = 0,
    Dimension1D = 1,
    Dimension2D = 2,
    Dimension2DArray = 3,
    DimensionCube = 4,
    DimensionCubeArray = 5,
    Dimension3D = 6,
    DimensionForce32 = int.MaxValue
}