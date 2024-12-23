using System;
using System.Collections.Generic;
using System.Text;

namespace SilkWrapped.SourceGenerator.Common;
public static class CSharpStringBuilderExtensions
{
    public static IndentedStringBuilder BlockStart(this IndentedStringBuilder builder)
    {
        return builder.AppendLine("{").IncrementIndent();
    }

    public static IndentedStringBuilder BlockEnd(this IndentedStringBuilder builder)
    {
        return builder.DecrementIndent().AppendLine("}");
    }
}
