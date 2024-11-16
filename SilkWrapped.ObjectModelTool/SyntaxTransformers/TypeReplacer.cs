using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SilkWrapped.ObjectModelTool.SyntaxTransformers;
internal class TypeReplacer(IDictionary<TypeSyntax, TypeSyntax> typeMap) : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        TypeSyntax type = node.Declaration.Type;
        if (type is not null && typeMap.TryGetValue(type.WithoutTrivia(), out var replacement))
        {
            if (type is PointerTypeSyntax && replacement is not NullableTypeSyntax)
            {
                replacement = NullableType(replacement);
            }

            return node.WithDeclaration(node.Declaration.WithType(replacement.WithTriviaFrom(type)));
        }

        return base.VisitFieldDeclaration(node);
    }

    public override SyntaxNode? VisitParameter(ParameterSyntax node)
    {
        if (node.Type is not null && typeMap.TryGetValue(node.Type.WithoutTrivia(), out var replacement))
        {
            if (node.Type is PointerTypeSyntax && replacement is not NullableTypeSyntax)
            {
                replacement = NullableType(replacement);
            }

            return node.WithType(replacement.WithTriviaFrom(node.Type));
        }

        return base.VisitParameter(node);
    }

    public override SyntaxNode? VisitPointerType(PointerTypeSyntax node)
    {
        if (node.ElementType is TypeSyntax typeSyntax && typeMap.TryGetValue(typeSyntax.WithoutTrivia(), out var replacement))
        {
            return node.WithElementType(replacement.WithTriviaFrom(typeSyntax));
        }


        return base.VisitPointerType(node);
    }
}
