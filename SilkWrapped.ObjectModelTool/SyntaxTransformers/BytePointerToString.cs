using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SilkWrapped.ObjectModelTool.SyntaxTransformers;

internal class BytePointerToString(params string[] names) : CSharpSyntaxRewriter
{
    private readonly HashSet<string> names = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
    private readonly TypeSyntax stringSyntax = ParseTypeName("string? ");
    public override SyntaxNode? VisitVariableDeclaration(VariableDeclarationSyntax node)
    {
        if (node.Type is PointerTypeSyntax pointerType &&
            pointerType.ElementType.ToFullString() == "byte")
        {
            return node.WithType(stringSyntax);
        }

        return base.VisitVariableDeclaration(node);
    }

    public override SyntaxNode? VisitParameter(ParameterSyntax node)
    {
        if (node.Type is PointerTypeSyntax pointerType &&
            pointerType.ElementType.ToFullString() == "byte")
        {
            return node.WithType(stringSyntax);
        }

        return base.VisitParameter(node);
    }
}
