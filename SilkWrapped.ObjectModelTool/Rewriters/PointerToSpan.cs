using Humanizer;
using Microsoft.CodeAnalysis.CSharp;

namespace SilkWrapped.ObjectModelTool.Rewriters;
internal class PointerToSpan : CSharpSyntaxRewriter
{

    private readonly Dictionary<string, FieldDeclarationSyntax?> fieldUpdates = [];

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


            fieldUpdates[countField] = null;
            fieldUpdates[pointerName] = pointerField.WithDeclaration(pointerField.Declaration.WithType(newType));


            makeRef = true;
        }

        if (makeRef)
        {
            node = (StructDeclarationSyntax)MakeRefStruct.Instance.Visit(node);
        }

        return base.VisitStructDeclaration(node);
    }

    public override SyntaxNode? VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        if (fieldUpdates.TryGetValue(node.Declaration.Variables[0].Identifier.Text, out var fieldUpdate))
        {
            return fieldUpdate;
        }

        return base.VisitFieldDeclaration(node);
    }

    public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        node = (ConstructorDeclarationSyntax)VisitBaseMethodDeclarationSyntax(node);
        return base.VisitConstructorDeclaration(node);
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        node = (MethodDeclarationSyntax)VisitBaseMethodDeclarationSyntax(node);
        return base.VisitMethodDeclaration(node);
    }

    protected SyntaxNode VisitBaseMethodDeclarationSyntax<T>(T node)
        where T : BaseMethodDeclarationSyntax
    {
        var parameters = node.ParameterList.Parameters.ToDictionary(n => n.Identifier.ToString());

        foreach (var parameter in parameters.Keys.Where(k => k.EndsWith("Count")))
        {
            var pointerName = parameter.Substring(0, parameter.Length - "Count".Length).Pluralize();

            if (!parameters.TryGetValue(pointerName, out var pointerParameter)) continue;
            if (pointerParameter.Type is not PointerTypeSyntax pointerType) continue;

            var newType = ParseTypeName($"ReadOnlySpan<{pointerType.ElementType.ToString()}> ");

            var newParam = pointerParameter.WithType(newType);

            if (pointerParameter.Default is not null)
            {
                newParam = newParam.WithDefault(EqualsValueClause(ParseExpression(" default")));
            }

            pointerParameter
                = node.ParameterList.Parameters.First(n => n.Identifier.ToString() == pointerName);

            node = node!.ReplaceNode(pointerParameter, newParam);

            ParameterSyntax countParameterNode
                = node.ParameterList.Parameters.First(n => n.Identifier.ToString() == parameter);

            node = node!.RemoveNode(countParameterNode, SyntaxRemoveOptions.KeepLeadingTrivia)!;

            var body = node.Body;

            var statementToUpdates = from statement in body!.Statements.OfType<IfStatementSyntax>()
                                     from identifier in statement.Condition.DescendantNodes().OfType<IdentifierNameSyntax>()
                                     where identifier.ToString() == pointerName
                                     select statement;

            body = body.ReplaceNodes(statementToUpdates, (old, _) => old.WithCondition(ParseExpression($"{pointerName}.Length > 0")));

            var statementToRemoves = from statement in body!.Statements.OfType<IfStatementSyntax>()
                                     from identifier in statement.Condition.DescendantNodes().OfType<IdentifierNameSyntax>()
                                     where node.ParameterList.Parameters.Any(p => parameter == identifier.ToString())
                                     select statement;

            body = body.RemoveNodes(statementToRemoves, SyntaxRemoveOptions.KeepEndOfLine);

            node = (T)node.WithBody(body);

        }
        return node;
    }
}
