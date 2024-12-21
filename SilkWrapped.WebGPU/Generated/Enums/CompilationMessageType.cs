using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public enum CompilationMessageType
{
    Error = 0,
    Warning = 1,
    Info = 2,
    Force32 = int.MaxValue
}