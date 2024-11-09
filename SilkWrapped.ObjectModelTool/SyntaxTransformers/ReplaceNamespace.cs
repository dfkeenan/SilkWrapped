using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SilkWrapped.ObjectModelTool.SyntaxTransformers;
internal class ReplaceNamespace(string from, string to) : CSharpSyntaxRewriter
{
    private NameSyntax toName = ParseName(to);
    public override SyntaxNode? VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        if (node.Name.ToFullString() == from)
        {
            return node.WithName(toName);
        }

        return base.VisitNamespaceDeclaration(node);
    }

    public override SyntaxNode? VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
    {
        if(node.Name.ToFullString() == from)
        {
            return node.WithName(toName);
        }

        return base.VisitFileScopedNamespaceDeclaration(node);
    }
}
