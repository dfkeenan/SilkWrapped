using Humanizer;
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
        var insertAt = statements.Length;
        bool allocRequired = false;

        //Add handle
        arguments.Add(node.ParameterList.Parameters[0].Identifier.Text);

        foreach (var parameter in node.ParameterList.Parameters.Skip(1))
        {
            var parameterName = parameter.Identifier.Text;
            var argument = parameterName;

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

                    statements.AppendLine($"{apiTypeSymbol.ToDisplayString()} {argument} = default;");

                    if (parameter.Modifiers is { Count: 0 } || parameter.Modifiers.ToString() != "ref")
                    {
                        if (!Context.TryGetGeneratedTypeSymbol(typeName, out var generatedTypeSymbol))
                            throw new InvalidOperationException($"No symbol found for {typeName}");

                        var left = argument;

                        var mappingContext = new MappingContext(
                            Context,
                            apiNamespace,
                            statements,
                            left,
                            parameterName,
                            generatedTypeSymbol);

                        allocRequired = MapMembers(mappingContext) || allocRequired;
                    }
                    else
                    {
                        outParameters.Add(parameter);
                    }
                }
            }

            if(parameter.Modifiers is { Count: > 0 })
            {
                argument = $"{parameter.Modifiers.ToString()} {argument}";
            }

            arguments.Add(argument);
        }

        if (allocRequired)
        {
            statements.InsertLine(insertAt, "using var m = new MarshalHelper();");
        }


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

        foreach (var parameter in outParameters)
        {
            statements.AppendLine($"//TODO: Map output {parameter.Identifier}");
        }

        if (returnStatement != null)
        {
            statements.AppendLine(returnStatement);
        }

        statements.DecrementIndent();
        statements.AppendLine("}");

        return ParseMemberDeclaration(statements.ToString()) as MethodDeclarationSyntax;

    }

    private record MappingContext(
        GeneratorTransformContext Context,
        string ApiNamespace, 
        IndentedStringBuilder Statements, 
        string Left, 
        string Right, 
        ITypeSymbol MemberType, 
        string? MemberName = null, 
        char Index = 'i');

    private static bool MapMembers(MappingContext mappingContext)
    {
        var allocRequired = false;

        foreach (var member in mappingContext.MemberType.GetMembers().OfType<IFieldSymbol>())
        {
            var memberMappingContext = mappingContext with
            {
                Left = $"{mappingContext.Left}.{member.Name}",
                Right = $"{mappingContext.Right}.{member.Name}",
                MemberType = member.Type,
                MemberName = member.Name
            };

            allocRequired = MapMember(memberMappingContext) || allocRequired;
        }

        return allocRequired;
    }


    private static bool MapMember(MappingContext mappingContext)
    {
        var allocRequired = false;

        if (mappingContext.MemberType is IArrayTypeSymbol arrayType && arrayType.ElementType is INamedTypeSymbol et)
        {
            var memberType = et;

            var childLeft = mappingContext.Left;
            var childRight = mappingContext.Right;

            mappingContext.Statements.AppendLine($"if ({childRight} != null) {{");
            mappingContext.Statements.IncrementIndent();
            var count = childLeft.Singularize() + "Count";
            mappingContext.Statements.AppendLine($"{count} = (nuint){childRight}!.Length;");
            var apiType = $"{mappingContext.ApiNamespace}.{memberType.Name}";
            mappingContext.Statements.AppendLine($"{childLeft} = m.AllocatePtr<{apiType}>({childRight}.Length);");

            mappingContext.Statements.AppendLine($"for (int {mappingContext.Index} = 0; {mappingContext.Index} < {childRight}!.Length; {mappingContext.Index}++)");
            mappingContext.Statements.AppendLine("{");
            mappingContext.Statements.IncrementIndent();
            childLeft = $"{childLeft}[{mappingContext.Index}]";
            childRight = $"{childRight}![{mappingContext.Index}]";

            var arrayMappingContext = mappingContext with
            {
                Left = childLeft,
                Right = childRight,
                MemberType = memberType,
                MemberName = "",
                Index = (char)(mappingContext.Index + 1)
            };

            allocRequired = MapMember(arrayMappingContext) || allocRequired;

            mappingContext.Statements.DecrementIndent();
            mappingContext.Statements.AppendLine("}");
            mappingContext.Statements.DecrementIndent();
            mappingContext.Statements.AppendLine("}");
            allocRequired = true;
            
        }
        else if (mappingContext.MemberType is INamedTypeSymbol namedMemberType)
        {
            var memberExpression = mappingContext.Right;

            if (namedMemberType.TypeKind == TypeKind.Enum)
            {
                memberExpression = $"({mappingContext.ApiNamespace}.{namedMemberType.Name}){memberExpression}";
                mappingContext.Statements.AppendLine($"{mappingContext.Left} = {memberExpression};");
            }
            else if (namedMemberType.SpecialType == SpecialType.System_String)
            {
                memberExpression = $"m.Utf8Ptr({memberExpression})";
                mappingContext.Statements.AppendLine($"{mappingContext.Left} = {memberExpression};");
                allocRequired = true;
            }
            else if (namedMemberType.TypeKind == TypeKind.Struct && 
                     mappingContext.Context.IsGeneratedTypeSymbol(namedMemberType) &&
                     mappingContext.Context.ShouldMarshall(namedMemberType.Name))
            {
                var childLeft = mappingContext.Left;
                var childRight = memberExpression;

                if (namedMemberType.IsNullableOfT(out var nullableType))
                {
                    namedMemberType = nullableType;
                    var childMemberName = childLeft.Replace('.', '_').Replace('[', '_').Replace(']','_');

                    mappingContext.Statements.AppendLine($"{mappingContext.ApiNamespace}.{namedMemberType.Name} {childMemberName} = default;");
                    mappingContext.Statements.AppendLine($"if ({childRight}.HasValue) {{");
                    mappingContext.Statements.IncrementIndent();
                    childLeft = childMemberName;
                    childRight = $"{childRight}!.Value";

                    var memberMappingContext = mappingContext with
                    {
                        Left = childLeft,
                        Right = childRight,
                        MemberType = namedMemberType,
                    };

                    allocRequired = MapMembers(memberMappingContext) || allocRequired;

                    mappingContext.Statements.AppendLine($"{mappingContext.Left} = &{childMemberName}; //PINREF");
                    mappingContext.Statements.DecrementIndent();
                    mappingContext.Statements.AppendLine("}");
                }
                else
                {
                    allocRequired = MapMembers(mappingContext) || allocRequired;
                }
            }
            else
            {
                mappingContext.Statements.AppendLine($"{mappingContext.Left} = {memberExpression};");
            }
        }

        

        return allocRequired;
    }
}
