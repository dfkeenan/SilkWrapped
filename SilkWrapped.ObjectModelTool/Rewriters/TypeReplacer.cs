using Microsoft.CodeAnalysis.CSharp;

namespace SilkWrapped.ObjectModelTool.Rewriters;
internal class TypeReplacer(TypeSyntax fromType, TypeSyntax toType, bool skipConstructor = false) : CSharpSyntaxRewriter
{
    private readonly TypeSyntax fromType = fromType.WithoutTrivia();
    private readonly TypeSyntax toType = toType.WithoutTrivia();
    public override SyntaxNode? VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        TypeSyntax type = node.Declaration.Type;
        if (type is not null && type.WithoutTrivia().IsEquivalentTo(fromType))
        {
            var replacement = toType;
            //if (type is PointerTypeSyntax && replacement is not NullableTypeSyntax)
            //{
            //    replacement = NullableType(replacement);
            //}

            return node.WithDeclaration(node.Declaration.WithType(replacement.WithTriviaFrom(type)));
        }

        return base.VisitFieldDeclaration(node);
    }

    private bool isConstructor;

    public override SyntaxNode? VisitParameterList(ParameterListSyntax node)
    {
        isConstructor = node.Ancestors().OfType<ConstructorDeclarationSyntax>().Any();

        return base.VisitParameterList(node);
    }

    public override SyntaxNode? VisitParameter(ParameterSyntax node)
    {
        if (skipConstructor && node.Ancestors().OfType<ConstructorDeclarationSyntax>().Any())
        {
            return base.VisitParameter(node);
        }

        if (node.Type is not null && node.Type.WithoutTrivia().IsEquivalentTo(fromType))
        {
            var replacement = toType;
            if (isConstructor && node.Type is PointerTypeSyntax && replacement is not NullableTypeSyntax)
            {
                replacement = NullableType(replacement);
            }

            return node.WithType(replacement.WithTriviaFrom(node.Type));
        }

        return base.VisitParameter(node);
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (node.ReturnType is not null && node.ReturnType.WithoutTrivia().IsEquivalentTo(fromType))
        {
            var replacement = toType;
            //if (node.ReturnType is PointerTypeSyntax && replacement is not NullableTypeSyntax)
            //{
            //    replacement = NullableType(replacement);
            //}

            node = node.WithReturnType(replacement.WithTriviaFrom(node.ReturnType));
        }

        var result = base.VisitMethodDeclaration(node);

        return result;
    }

    public override SyntaxNode? VisitPointerType(PointerTypeSyntax node)
    {
        var replacement = toType;
        if (node.ElementType is TypeSyntax typeSyntax && typeSyntax.WithoutTrivia().IsEquivalentTo(fromType))
        {
            return node.WithElementType(replacement.WithTriviaFrom(typeSyntax));
        }

        return base.VisitPointerType(node);
    }
}
