using Microsoft.CodeAnalysis.CSharp;

namespace SilkWrapped.ObjectModelTool.Rewriters;

internal class SimplifyMethodSignature : CSharpSyntaxRewriter
{
    private string? argumentExpression;

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        var firstParameter = node.ParameterList.Parameters[0];

        if (firstParameter is null)
        {
            return base.VisitMethodDeclaration(node);
        }

        argumentExpression = firstParameter.Identifier.Text;

        var name = TypeName(firstParameter.Type);

        if (name is null)
        {
            return base.VisitMethodDeclaration(node);
        }

        var methodName = node.Identifier.Text.Replace(name, "");

        node = node.WithIdentifier(Identifier(methodName))
                   .WithParameterList(node.ParameterList.WithParameters(node.ParameterList.Parameters.RemoveAt(0)));

        return base.VisitMethodDeclaration(node);
    }

    public override SyntaxNode? VisitArgument(ArgumentSyntax node)
    {
        if (node.Expression is IdentifierNameSyntax expression && expression.Identifier.Text == argumentExpression)
        {
            return node.WithExpression(IdentifierName("Handle").WithTriviaFrom(expression));
        }

        return base.VisitArgument(node);
    }
}
