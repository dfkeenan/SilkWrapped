using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SilkWrapped.ObjectModelTool.SyntaxTransformers;
internal class RemoveAttributes(params string[] names) : CSharpSyntaxRewriter
{
    private readonly HashSet<string> names = new HashSet<string>(names);
    public override SyntaxNode? VisitAttribute(AttributeSyntax node)
    {
        if (names.Contains(node.Name.ToFullString()))
        {
            return null;
        }

        return base.VisitAttribute(node);
    }

    public override SyntaxNode? VisitAttributeList(AttributeListSyntax node)
    {
        var visitedNode = base.VisitAttributeList(node) as AttributeListSyntax;

        if (visitedNode?.Attributes is null or [])
        {
            return null;
        }

        return visitedNode;
    }
}
