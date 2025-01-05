namespace SilkWrapped.SourceGenerator.Common;
public static class CSharpIndentedStringBuilderExtensions
{
    public static IndentedStringBuilder BlockStart(this IndentedStringBuilder builder, char open = '{')
    {
        return builder.Append(open).AppendLine().IncrementIndent();
    }

    public static IndentedStringBuilder BlockEnd(this IndentedStringBuilder builder, char close = '}')
    {
        return builder.DecrementIndent().Append(close).AppendLine();
    }

    public static IDisposable BeginBlock(this IndentedStringBuilder builder, char open = '{', char? separator = null)
    {
        var close = open switch
        {
            '{' => '}',
            '[' => ']',
            _ => throw new ArgumentException("Unable to determine closing token", nameof(open)),
        };

        builder.Append(open).AppendLine().IncrementIndent();

        return new BlockSuspender(builder, close, separator);
    }

    public static IndentedStringBuilder AppendCompilerGenerated(
        this IndentedStringBuilder builder,
        string generatorName,
        bool includeNonUserCodeAttributes = true)
    {
        // We can use this class to get the assembly, as all files for generators are just included
        // via shared projects. As such, the assembly will be the same as the generator type itself.
        Version assemblyVersion = typeof(IndentedStringBuilder).Assembly.GetName().Version;

        builder.AppendLine($"""[{CommonNamespaces.Compiler["GeneratedCode"]}("{generatorName}", "{assemblyVersion}")]""");

        if (includeNonUserCodeAttributes)
        {
            builder.AppendLine($"""[{CommonNamespaces.Diagnostics["DebuggerNonUserCode"]}]""");
            builder.AppendLine($"""[{CommonNamespaces.Diagnostics["CodeAnalysis.ExcludeFromCodeCoverage"]}]""");
        }

        return builder;
    }

    public static IndentedStringBuilder AppendNeverEditorBrowsable(this IndentedStringBuilder builder)
    {
        return builder.AppendLine($"[{CommonNamespaces.ComponentModel["EditorBrowsable"]}({CommonNamespaces.ComponentModel["EditorBrowsableState"]}.Never)]");
    }

    private sealed class BlockSuspender(IndentedStringBuilder builder, char close, char? separator) : IDisposable
    {
        public void Dispose()
        {
            builder.DecrementIndent().Append(close);
            if (separator.HasValue)
            {
                builder.Append(separator.Value);
            }
            builder.AppendLine();
        }
    }
}
