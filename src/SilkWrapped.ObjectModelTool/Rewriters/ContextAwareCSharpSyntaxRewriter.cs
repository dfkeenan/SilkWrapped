using System.Diagnostics.CodeAnalysis;

namespace SilkWrapped.ObjectModelTool.Rewriters;
internal class ContextAwareCSharpSyntaxRewriter : CSharpSyntaxRewriter
{
    protected GeneratorTransformContext Context { get; set; } = default!;

    public static string ToPascalCase(string s)
    {
        return Char.ToLowerInvariant(s[0]) + s[1..];
    }

    [return: NotNullIfNotNull(nameof(node))]
    public virtual SyntaxNode? Visit(SyntaxNode? node, GeneratorTransformContext context)
    {
        Context = context;

        return Visit(node);
    }
}
