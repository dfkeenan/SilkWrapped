using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SilkWrapped.ObjectModelTool.SyntaxTransformers;
internal class RemoveChainingPointers : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        if (TypeIsChangedStruct(node.Declaration.Type))
        {
            return null;
        }

        return base.VisitFieldDeclaration(node);
    }

    public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        var parameters = node.ParameterList.Parameters.Where(p => TypeIsChangedStruct(p.Type!)).ToList();

        if (parameters.Any())
        {
            node = node.WithParameterList(node.ParameterList.RemoveNodes(parameters, SyntaxRemoveOptions.KeepEndOfLine)!);

            var body = node.Body;

            var statementsToRemove = from statement in body!.Statements.OfType<IfStatementSyntax>()
                                     from identifier in statement.DescendantNodes().OfType<IdentifierNameSyntax>()
                                     where parameters.Any(p => p.Identifier.ToString() == identifier.ToString())
                                     select statement;
            body = body.RemoveNodes(statementsToRemove, SyntaxRemoveOptions.KeepEndOfLine);

            node = node.WithBody(body);

            return node;
        }

        return base.VisitConstructorDeclaration(node);
    }

    private static bool TypeIsChangedStruct(TypeSyntax type)
    {
        return type is PointerTypeSyntax pointerType &&
            pointerType.ElementType.ToFullString() is string typeName &&
            (typeName == "" | typeName == "ChainedStruct");
    }
}
