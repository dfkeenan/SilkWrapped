using Humanizer;
using Microsoft.CodeAnalysis.CSharp;

namespace SilkWrapped.ObjectModelTool.Rewriters;
internal class PointerToSpan : CSharpSyntaxRewriter
{

    private readonly Dictionary<string, FieldDeclarationSyntax?> fieldUpdates = [];
    private readonly List<string> conditionReplacements = [];
    private readonly List<string> conditionRemovals = [];
    private readonly Dictionary<string, string> argumentReplacements = [];

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
        var updated = (ConstructorDeclarationSyntax?)VisitBaseMethodDeclarationSyntax(node);
        return updated is null ? null : base.VisitConstructorDeclaration(updated);
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        var updated = (MethodDeclarationSyntax?)VisitBaseMethodDeclarationSyntax(node);
        return updated is null ? null : base.VisitMethodDeclaration(updated);
    }

    protected SyntaxNode? VisitBaseMethodDeclarationSyntax<T>(T node)
        where T : BaseMethodDeclarationSyntax
    {
        var parameters = node.ParameterList.Parameters.ToDictionary(n => n.Identifier.ToString());

        conditionReplacements.Clear();
        conditionRemovals.Clear();
        argumentReplacements.Clear();

        foreach (var parameter in parameters.Keys.Where(k => k.EndsWith("Count")))
        {
            var pointerName = parameter.Substring(0, parameter.Length - "Count".Length).Pluralize();

            if (!parameters.TryGetValue(pointerName, out var pointerParameter)) continue;

            //Remove non-pointer overloads
            if (pointerParameter.Modifiers.Any(m => m.IsKind(SyntaxKind.ReferenceKeyword) || m.IsKind(SyntaxKind.InKeyword)))
            {
                return null;
            }

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

            conditionReplacements.Add(pointerName);
            conditionRemovals.Add(parameter);
            argumentReplacements[parameter] = $"({countParameterNode.Type!.ToString()}){pointerName}.Length";
        }
        return node;
    }

    public override SyntaxNode? VisitIfStatement(IfStatementSyntax node)
    {
        var conditionIdentifiers = node.Condition
                                       .DescendantNodes()
                                       .OfType<IdentifierNameSyntax>()
                                       .Select(i => i.Identifier.Text);

        var pointerName = conditionIdentifiers.FirstOrDefault(i => conditionReplacements.Contains(i));

        if (pointerName is not null)
        {
            node = node.WithCondition(ParseExpression($"{pointerName}.Length > 0"));
        }

        if (conditionIdentifiers.Any(i => conditionRemovals.Contains(i)))
        {
            return null;
        }

        return base.VisitIfStatement(node);
    }

    public override SyntaxNode? VisitArgument(ArgumentSyntax node)
    {
        if (argumentReplacements.TryGetValue(node.ToString(), out var replacement))
        {
            node = node.WithExpression(ParseExpression(replacement));
        }
        return base.VisitArgument(node);
    }
}
