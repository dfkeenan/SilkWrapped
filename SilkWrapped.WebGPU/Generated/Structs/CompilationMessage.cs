using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct CompilationMessage
{
    public unsafe string? Message;
    public CompilationMessageType Type;
    public ulong LineNum;
    public ulong LinePos;
    public ulong Offset;
    public ulong Length;
    public ulong Utf16LinePos;
    public ulong Utf16Offset;
    public ulong Utf16Length;
    public unsafe CompilationMessage(string? message = null, CompilationMessageType? type = null, ulong? lineNum = null, ulong? linePos = null, ulong? offset = null, ulong? length = null, ulong? utf16LinePos = null, ulong? utf16Offset = null, ulong? utf16Length = null)
    {
        this = default(CompilationMessage);
        if (message != null)
        {
            Message = message;
        }

        if (type.HasValue)
        {
            Type = type.Value;
        }

        if (lineNum.HasValue)
        {
            LineNum = lineNum.Value;
        }

        if (linePos.HasValue)
        {
            LinePos = linePos.Value;
        }

        if (offset.HasValue)
        {
            Offset = offset.Value;
        }

        if (length.HasValue)
        {
            Length = length.Value;
        }

        if (utf16LinePos.HasValue)
        {
            Utf16LinePos = utf16LinePos.Value;
        }

        if (utf16Offset.HasValue)
        {
            Utf16Offset = utf16Offset.Value;
        }

        if (utf16Length.HasValue)
        {
            Utf16Length = utf16Length.Value;
        }
    }
}