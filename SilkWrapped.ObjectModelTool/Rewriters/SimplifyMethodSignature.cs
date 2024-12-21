namespace SilkWrapped.ObjectModelTool.Rewriters;
internal class SimplifyMethodSignature : ContextAwareCSharpSyntaxRewriter
{
    private readonly Dictionary<string, ParameterSyntax> replacementParameters = [];
    private readonly Dictionary<string, ExpressionSyntax> replacementExpressions = [];
    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        replacementParameters.Clear();
        replacementExpressions.Clear();

        if (node.ParameterList.Parameters is [var parameter])
        {
            if (TypeName(parameter.Type) is string parameterTypeName &&
                Context.TryGetGeneratedTypeSymbol(parameterTypeName, out var parameterType))
            {
                if (parameterType.GetMembers().OfType<IFieldSymbol>().ToList() is [IFieldSymbol fieldSymbol] &&
                    fieldSymbol.Type.SpecialType == SpecialType.System_String)
                {
                    var p = ParseParameterList($"string? {ToPascalCase(fieldSymbol.Name)} = null")
                                .Parameters.First();

                    if (p != null)
                    {
                        replacementParameters.Add(parameter.Identifier.Text, p);
                        replacementExpressions.Add($"{parameter.Identifier}.{fieldSymbol.Name}", ParseExpression(ToPascalCase(fieldSymbol.Name)));
                    }
                }
            }
        }

        return base.VisitMethodDeclaration(node);
    }

    public override SyntaxNode? VisitParameter(ParameterSyntax node)
    {
        if (replacementParameters.TryGetValue(node.Identifier.Text, out var parameter))
        {
            return parameter;
        }

        return base.VisitParameter(node);
    }

    public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        if (replacementExpressions.TryGetValue(node.ToString(), out var expression))
        {
            return expression;
        }

        return base.VisitMemberAccessExpression(node);
    }
}
