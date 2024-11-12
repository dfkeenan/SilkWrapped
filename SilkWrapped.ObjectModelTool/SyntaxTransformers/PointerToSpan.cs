using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SilkWrapped.ObjectModelTool.SyntaxTransformers;
internal class PointerToSpan : CSharpSyntaxRewriter
{
    private readonly SyntaxToken refToken = ParseToken("ref ");
    public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node)
    {
        bool makeRef = false;

        var fields = node.Members.OfType<FieldDeclarationSyntax>()
                                 .ToDictionary(n => n.Declaration.Variables[0].Identifier.ToString());

        foreach (var countField in fields.Keys.Where(k => k.EndsWith("Count")))
        {
            var pointerName = countField.Substring(0, countField.Length - "Count".Length).Pluralize();

            if (!fields.TryGetValue(pointerName, out var pointerField)) continue;
            if (pointerField.Declaration.Type is not PointerTypeSyntax pointerType) continue;

            var newType = ParseTypeName($"ReadOnlySpan<{pointerType.ElementType.ToString()}> ");

            node = node!.ReplaceNode(pointerType, newType);

            FieldDeclarationSyntax countFieldNode
                = node.Members.OfType<FieldDeclarationSyntax>()
                              .First(n => n.Declaration.Variables[0].Identifier.ToString() == countField);
            node = node!.RemoveNode(countFieldNode, SyntaxRemoveOptions.KeepLeadingTrivia)!;

            makeRef = true;
        }

        if (makeRef)
        {
            node = node.AddModifiers(refToken);
        }

        return base.VisitStructDeclaration(node);
    }

    public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        var parameters = node.ParameterList.Parameters.ToDictionary(n => n.Identifier.ToString());

        foreach (var parameter in parameters.Keys.Where(k => k.EndsWith("Count")))
        {
            var pointerName = parameter.Substring(0, parameter.Length - "Count".Length).Pluralize();

            if (!parameters.TryGetValue(pointerName, out var pointerParameter)) continue;
            if (pointerParameter.Type is not PointerTypeSyntax pointerType) continue;

            var newType = ParseTypeName($"ReadOnlySpan<{pointerType.ElementType.ToString()}> ");

            var newParm = pointerParameter.WithType(newType).WithDefault(EqualsValueClause(ParseExpression(" default")));

            node = node!.ReplaceNode(pointerParameter, newParm);

            ParameterSyntax countFieldNode
                = node.ParameterList.Parameters.First(n => n.Identifier.ToString() == parameter);

            node = node!.RemoveNode(countFieldNode, SyntaxRemoveOptions.KeepLeadingTrivia)!;

            var body = node.Body;

            var statementsToRemove = from statement in body!.Statements.OfType<IfStatementSyntax>()
                                     from identifier in statement.Condition.DescendantNodes().OfType<IdentifierNameSyntax>()
                                     where node.ParameterList.Parameters.Any(p => parameter == identifier.ToString())
                                     select statement;
            body = body.RemoveNodes(statementsToRemove, SyntaxRemoveOptions.KeepEndOfLine);

            node = node.WithBody(body);

        }

        return base.VisitConstructorDeclaration(node);
    }
}
