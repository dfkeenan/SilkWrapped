using System.CommandLine;
using System.Reflection.Metadata;
using SilkWrapped.SourceGenerator;

namespace SilkWrapped.ObjectModelTool.Rewriters;

internal class ImplemementApiMethod : ContextAwareCSharpSyntaxRewriter
{
    private readonly RemoveAttributes removeAttributes = new RemoveAttributes();


    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {

        var arguments = new List<string>();
        var statements = new List<string>();

        //Add handle
        arguments.Add(node.ParameterList.Parameters[0].Identifier.Text);

        foreach (var parameter in node.ParameterList.Parameters.Skip(1))
        {
            var argument = parameter.Identifier.Text;

            var typeName = TypeName(parameter.Type);

            if (typeName is not null && Context.TryGetApiTypeSymbol(typeName, out var apiTypeSymbol)) 
            { 
                if(apiTypeSymbol.TypeKind == TypeKind.Enum)
                {
                    if(parameter.Modifiers.ToString() == "ref")
                    {
                        argument = $"Unsafe.As<{apiTypeSymbol.Name},{apiTypeSymbol.ToDisplayString()}>(ref features)";
                    }
                    else
                    {
                        argument = $"({apiTypeSymbol.ToDisplayString()}){argument}";
                    }
                }
                else if(apiTypeSymbol.TypeKind == TypeKind.Struct && Context.ShouldMarshall(apiTypeSymbol.Name))
                {
                    argument = $"__{argument}";

                    statements.Add($"{apiTypeSymbol.ToDisplayString()} {argument} = default;");

                }
            
            
            }

            if(parameter.Modifiers is { Count: > 0 })
            {
                argument = $"{parameter.Modifiers.ToString()} {argument}";
            }

            arguments.Add(argument);
        }


        TypeParameterListSyntax? typeParameterList
            = node.TypeParameterList is null
            ? null
            : (TypeParameterListSyntax)removeAttributes.Visit(node.TypeParameterList);

        var callStatement = $"{Context.ApiName}.{node.Identifier.Text}{typeParameterList?.ToString()}({string.Join(", ", arguments)});";

        if (node.ReturnType.IsVoid())
        {
            statements.Add(callStatement);
        }
        else
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

            var returnStatement = $"return {resultVariable};";

            statements.Add(callStatement);
            statements.Add(returnStatement);
        }


        return node.WithBody(node.Body!.AddStatements(
            statements.Select(s => ParseStatement(s))
            ));

    }
}
