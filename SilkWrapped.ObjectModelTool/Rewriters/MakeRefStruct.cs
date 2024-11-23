using Microsoft.CodeAnalysis.CSharp;

namespace SilkWrapped.ObjectModelTool.Rewriters;
internal class MakeRefStruct : CSharpSyntaxRewriter
{
    private static readonly SyntaxToken refToken = ParseToken("ref ");
    public static MakeRefStruct Instance { get; } = new MakeRefStruct();
    public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node)
    {

        if (node.Modifiers.IndexOf(SyntaxKind.PartialKeyword) is int index and >= 0)
        {
            node = node.WithModifiers(node.Modifiers.Insert(index, refToken));
        }
        else
        {
            node = node.AddModifiers(refToken);
        }

        return base.VisitStructDeclaration(node);
    }
}
