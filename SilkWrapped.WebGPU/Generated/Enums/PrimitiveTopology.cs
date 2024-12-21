using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public enum PrimitiveTopology
{
    PointList = 0,
    LineList = 1,
    LineStrip = 2,
    TriangleList = 3,
    TriangleStrip = 4,
    Force32 = int.MaxValue
}