using SilkWrapped.SourceGenerator;

namespace SilkWrapped.ObjectModelTool.Rewriters;

internal class ImplemementApiMethod : ContextAwareCSharpSyntaxRewriter
{
    private readonly RemoveAttributes removeAttributes = new RemoveAttributes();


    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {

        var apiNamespace = Context.ApiTypeSymbol.ContainingNamespace.ToDisplayString();
        var arguments = new List<string>();
        var outParameters = new List<ParameterSyntax>();
        var statements = new IndentedStringBuilder();
        statements.AppendLine(node.WithBody(null).ToFullString());
        statements.AppendLine("{");
        statements.IncrementIndent();

        //Add handle
        arguments.Add(node.ParameterList.Parameters[0].Identifier.Text);

        var allocated = MemberMapper.MapInParameters(
                        Context,
                        statements,
                        node.ParameterList.Parameters.Skip(1),
                        arguments,
                        outParameters);

        TypeParameterListSyntax? typeParameterList
            = node.TypeParameterList is null
            ? null
            : (TypeParameterListSyntax)removeAttributes.Visit(node.TypeParameterList);

        var callStatement = $"{Context.ApiName}.{node.Identifier.Text}{typeParameterList?.ToString()}({string.Join(", ", arguments)});";
        string? returnStatement = null;
        if (!node.ReturnType.IsVoid())
        {
            var resultVariable = "result";

            callStatement = $"var {resultVariable} = {callStatement}";

            if (TypeName(node.ReturnType) is string returnTypeName && Context.TryGetApiTypeSymbol(returnTypeName, out var apiTypeSymbol))
            {
                if (apiTypeSymbol.TypeKind == TypeKind.Enum)
                {
                    resultVariable = $"({apiTypeSymbol.Name}){resultVariable}";
                }
            }

            returnStatement = $"return {resultVariable};";
        }

        statements.AppendLine(callStatement);

        MemberMapper.MapOutParameters(
                    Context,
                    statements,
                    outParameters,
                    arguments,
                    false,
                    allocated);

        if (returnStatement != null)
        {
            statements.AppendLine(returnStatement);
        }

        statements.DecrementIndent();
        statements.AppendLine("}");

        return ParseMemberDeclaration(statements.ToString()) as MethodDeclarationSyntax;

    }




}
