using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SilkWrapped.ObjectModelTool.Rewriters;
internal class ContextAwareCSharpSyntaxRewriter : CSharpSyntaxRewriter
{
    protected GeneratorTransformContext Context { get; set; } = default!;

    [return: NotNullIfNotNull(nameof(node))]
    public virtual SyntaxNode? Visit(SyntaxNode? node, GeneratorTransformContext context)
    {
        Context = context;

        return Visit(node);
    }
}
