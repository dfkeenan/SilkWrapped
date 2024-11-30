using SilkWrapped.SourceGenerator;

namespace SilkWrapped.ObjectModelTool.Rewriters;

internal class ImplemementApiMethod : ContextAwareCSharpSyntaxRewriter
{
    private readonly RemoveAttributes removeAttributes = new RemoveAttributes();


    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {

        var arguments = from p in node.ParameterList.Parameters
                        select p.Modifiers switch
                        {
                            { Count: > 0 } => $"{p.Modifiers.ToString()} {p.Identifier.Text}",
                            _ => p.Identifier.Text
                        };



        TypeParameterListSyntax? typeParameterList
            = node.TypeParameterList is null
            ? null
            : (TypeParameterListSyntax)removeAttributes.Visit(node.TypeParameterList);

        var callStatement = $"{Context.ApiName}.{node.Identifier.Text}{typeParameterList?.ToString()}({string.Join(", ", arguments)});";

        if (node.ReturnType.IsVoid())
        {
            return node.WithBody(node.Body!.AddStatements(ParseStatement(callStatement)));
        }

        callStatement = $"var result = {callStatement}";
        var returnStatement = "return result;";
        return node.WithBody(node.Body!.AddStatements(
            ParseStatement(callStatement),
            ParseStatement(returnStatement)
            ));

    }
}
