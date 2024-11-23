using Microsoft.CodeAnalysis.CSharp;

namespace SilkWrapped.ObjectModelTool.Rewriters;

internal class AddObjectModelApi : ContextAwareCSharpSyntaxRewriter
{
    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        var firstParameter = node.Members.OfType<MethodDeclarationSyntax>().FirstOrDefault()?.ParameterList.Parameters[0];

        if (firstParameter is null)
        {
            return base.VisitClassDeclaration(node);
        }

        var apiProperty = PropertyDeclaration(Context.ApiTypeSymbol, "Api", SyntaxKind.PublicKeyword);

        node = node.WithMembers(node.Members.Insert(0, apiProperty));

        return node;
    }
}
