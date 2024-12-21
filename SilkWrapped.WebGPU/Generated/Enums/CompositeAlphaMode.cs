using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public enum CompositeAlphaMode
{
    Auto = 0,
    Opaque = 1,
    Premultiplied = 2,
    Unpremultiplied = 3,
    Inherit = 4,
    Force32 = int.MaxValue
}