using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SilkWrapped.ObjectModelTool.SyntaxTransformers;
internal class TypeReplacer(IDictionary<TypeSyntax, TypeSyntax> typeMap) : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        if (node.Declaration.Type is not null && typeMap.TryGetValue(node.Declaration.Type.WithoutTrivia(), out var replacement))
        {
            return node.WithDeclaration(node.Declaration.WithType(replacement.WithTriviaFrom(node.Declaration.Type)));
        }

        return base.VisitFieldDeclaration(node);
    }

    public override SyntaxNode? VisitParameter(ParameterSyntax node)
    {
        if (node.Type is not null && typeMap.TryGetValue(node.Type.WithoutTrivia(), out var replacement))
        {
            return node.WithType(replacement.WithTriviaFrom(node.Type));
        }

        return base.VisitParameter(node);
    }
}
