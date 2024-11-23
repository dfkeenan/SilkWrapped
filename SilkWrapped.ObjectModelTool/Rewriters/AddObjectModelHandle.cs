using Microsoft.CodeAnalysis.CSharp;

namespace SilkWrapped.ObjectModelTool.Rewriters;
internal class AddObjectModelHandle : ContextAwareCSharpSyntaxRewriter
{
    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        var firstParameter = node.Members.OfType<MethodDeclarationSyntax>().FirstOrDefault()?.ParameterList.Parameters[0];

        if (firstParameter is null)
        {
            return base.VisitClassDeclaration(node);
        }

        var handleType = ParseTypeName($"{Context.ApiTypeSymbol.ContainingNamespace.ToDisplayString()}.{firstParameter.Type!.ToFullString()}");

        var handleProperty = PropertyDeclaration(handleType, "Handle", SyntaxKind.PublicKeyword);

        node = node.WithMembers(node.Members.Insert(0, handleProperty));

        return node;
    }
}
