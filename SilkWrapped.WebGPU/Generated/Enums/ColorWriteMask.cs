using System;
using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
[Flags]
public enum ColorWriteMask
{
    None = 0,
    Red = 1,
    Green = 2,
    Blue = 4,
    Alpha = 8,
    All = 0xF,
    Force32 = int.MaxValue
}