using System.Diagnostics.CodeAnalysis;

namespace SilkWrapped.ObjectModelTool.Rewriters;
internal class ObjectModelTypeReplacer : ContextAwareCSharpSyntaxRewriter
{
    public bool SkipConstructor { get; set; }

    [return: NotNullIfNotNull("node")]
    public override SyntaxNode? Visit(SyntaxNode? node, GeneratorTransformContext context)
    {
        foreach (var item in context.Items.Where(i => i.IsObjectModel))
        {
            node = new TypeReplacer(item.SourceType, item.TypeSyntax, SkipConstructor).Visit(node);
        }
        return node;
    }
}
