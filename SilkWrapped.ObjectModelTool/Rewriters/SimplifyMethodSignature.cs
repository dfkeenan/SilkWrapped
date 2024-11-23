using Microsoft.CodeAnalysis.CSharp;

namespace SilkWrapped.ObjectModelTool.Rewriters;

internal class SimplifyMethodSignature : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        var firstParameter = node.ParameterList.Parameters[0];

        if (firstParameter is null)
        {
            return base.VisitMethodDeclaration(node);
        }

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
}
