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

        var members = node.Members.Insert(0, handleProperty);

        var castOperator = ParseMemberDeclaration($"public static implicit operator {handleType.ToString()}({node.Identifier.Text} obj) => obj.Handle;")!;
        members = members.Add(castOperator);

        node = node.WithMembers(members);

        return node;
    }
}
