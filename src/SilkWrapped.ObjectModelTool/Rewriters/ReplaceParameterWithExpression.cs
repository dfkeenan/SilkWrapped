using System.Text.Json.Serialization;

namespace SilkWrapped.ObjectModelTool.Rewriters;
internal class ReplaceParameterWithExpression : CSharpSyntaxRewriter, IJsonOnDeserialized
{
    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public Dictionary<string, string> Parameters { get; } = [];
    private readonly Dictionary<ParameterSyntax, string> parameters = new Dictionary<ParameterSyntax, string>(new IsEquivalentToEqualityComparer<ParameterSyntax>());
    private readonly Dictionary<string, string> arguments = [];

    public void OnDeserialized()
    {
        foreach (var (parameter, expression) in Parameters)
        {
            parameters[ParseParameterList(parameter).Parameters[0].WithoutTrivia()] = expression;
        }
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        arguments.Clear();
        var replacedParameters = new List<ParameterSyntax>();

        foreach (var parameter in node.ParameterList.Parameters)
        {
            if (parameters.TryGetValue(parameter.WithoutTrivia(), out var expression))
            {
                replacedParameters.Add(parameter);
                arguments[parameter.Identifier.Text] = expression;
            }
        }

        if (replacedParameters.Count > 0)
        {
            node = node.RemoveNodes(replacedParameters, SyntaxRemoveOptions.KeepNoTrivia)!;
        }

        return base.VisitMethodDeclaration(node);
    }

    public override SyntaxNode? VisitArgument(ArgumentSyntax node)
    {
        if (arguments.TryGetValue(node.Expression.ToString(), out var expression))
        {
            node = node.WithExpression(ParseExpression(expression));
        }

        return base.VisitArgument(node);
    }
}
