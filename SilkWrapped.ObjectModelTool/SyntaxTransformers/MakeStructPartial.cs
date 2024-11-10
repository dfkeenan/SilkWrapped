using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SilkWrapped.ObjectModelTool.SyntaxTransformers;
internal class MakeStructPartial : CSharpSyntaxRewriter
{
    private readonly SyntaxToken partialToken = ParseToken("partial ");

    public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node)
    {
        node = node.AddModifiers(partialToken);

        return base.VisitStructDeclaration(node);
    }
}
