using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct CompilationInfo
{
    public unsafe CompilationMessage[]? Messages;
    public unsafe CompilationInfo(CompilationMessage[]? messages = null)
    {
        this = default(CompilationInfo);
        if (messages != null)
        {
            Messages = messages;
        }
    }
}