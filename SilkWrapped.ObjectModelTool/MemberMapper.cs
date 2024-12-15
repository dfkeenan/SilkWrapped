using Humanizer;

namespace SilkWrapped.ObjectModelTool;

internal enum MappingDirection
{
    In,
    Out
}

internal record MappingContext(
        GeneratorTransformContext Context,
        string ApiNamespace,
        IndentedStringBuilder Statements,
        string Left,
        string Right,
        ITypeSymbol MemberType,
        string? MemberName = null,
        char Index = 'i',
        MappingDirection Direction = MappingDirection.In);

internal static class MemberMapper
{
    public static bool MapInParameters(
        GeneratorTransformContext context,
        IndentedStringBuilder statements,
        IEnumerable<ParameterSyntax> parameters,
        List<string> arguments,
        List<ParameterSyntax> outParameters)
    {
        var apiNamespace = context.ApiTypeSymbol.ContainingNamespace.ToDisplayString();
        bool allocRequired = false;
        var insertAt = statements.Length;

        foreach (var parameter in parameters)
        {
            var parameterName = parameter.Identifier.Text;
            var argument = parameterName;

            var typeName = TypeName(parameter.Type);

            if (typeName is not null && context.TryGetApiTypeSymbol(typeName, out var apiTypeSymbol))
            {
                if (apiTypeSymbol.TypeKind == TypeKind.Enum)
                {
                    if (parameter.Modifiers.ToString() == "ref")
                    {
                        argument = $"Unsafe.As<{apiTypeSymbol.Name},{apiTypeSymbol.ToDisplayString()}>(ref features)";
                    }
                    else
                    {
                        argument = $"({apiTypeSymbol.ToDisplayString()}){argument}";
                    }
                }
                else if (apiTypeSymbol.TypeKind == TypeKind.Struct && context.ShouldMarshall(apiTypeSymbol.Name))
                {
                    argument = $"__{argument}";

                    statements.AppendLine($"{apiTypeSymbol.ToDisplayString()} {argument} = default;");

                    if (parameter.Modifiers is { Count: 0 } || parameter.Modifiers.ToString() != "ref")
                    {
                        if (!context.TryGetGeneratedTypeSymbol(typeName, out var generatedTypeSymbol))
                            throw new InvalidOperationException($"No symbol found for {typeName}");

                        var left = argument;

                        var mappingContext = new MappingContext(
                            context,
                            apiNamespace,
                            statements,
                            left,
                            parameterName,
                            generatedTypeSymbol);

                        allocRequired = MemberMapper.MapMembers(mappingContext) || allocRequired;
                    }
                    else
                    {
                        outParameters.Add(parameter);
                    }
                }
            }
            else if (typeName == "string")
            {
                argument = $"m.Utf8Ptr({argument})";
                allocRequired = true;
            }

            if (parameter.Modifiers is { Count: > 0 })
            {
                argument = $"{parameter.Modifiers.ToString()} {argument}";
            }

            arguments.Add(argument);
        }

        if (allocRequired)
        {
            statements.InsertLine(insertAt, "using var m = new MarshalHelper();");
        }

        return allocRequired;
    }


    public static void MapOutParameters(
        GeneratorTransformContext context,
        IndentedStringBuilder statements,
        IEnumerable<ParameterSyntax> parameters,
        List<string> arguments,
        bool declareOutVariable = true,
        bool allocated = false)
    {
        var apiNamespace = context.ApiTypeSymbol.ContainingNamespace.ToDisplayString();
        bool allocRequired = false;
        var insertAt = statements.Length;

        foreach (var parameter in parameters)
        {
            var parameterName = parameter.Identifier.Text;
            var argument = parameterName;

            var typeName = TypeName(parameter.Type);

            if (typeName is not null && context.TryGetApiTypeSymbol(typeName, out var apiTypeSymbol))
            {
                if (apiTypeSymbol.TypeKind == TypeKind.Enum)
                {
                    if (parameter.Modifiers.ToString() == "ref")
                    {
                        argument = $"Unsafe.As<{apiTypeSymbol.ToDisplayString()},{apiTypeSymbol.Name}>(ref features)";
                    }
                    else
                    {
                        argument = $"({apiTypeSymbol.Name}){argument}";
                    }
                }
                else if (apiTypeSymbol.TypeKind == TypeKind.Struct && context.ShouldMarshall(apiTypeSymbol.Name))
                {
                    argument = $"__{argument}";

                    if (declareOutVariable)
                    {
                        statements.AppendLine($"{apiTypeSymbol.Name} {argument} = default;");
                    }

                    if (!context.TryGetGeneratedTypeSymbol(typeName, out var generatedTypeSymbol))
                        throw new InvalidOperationException($"No symbol found for {typeName}");

                    if (parameter.Type is NullableTypeSyntax nullable)
                    {
                        statements.AppendLine($"if ({parameterName} != null)")
                            .AppendLine("{")
                            .IncrementIndent();

                        var right = $"{parameterName}->";

                        var mappingContext = new MappingContext(
                                                   context,
                                                   apiNamespace,
                                                   statements,
                                                   argument,
                                                   right,
                                                   generatedTypeSymbol,
                                                   Direction: MappingDirection.Out);

                        allocRequired = MemberMapper.MapMembers(mappingContext) || allocRequired;

                        statements.DecrementIndent().AppendLine("}");
                    }
                    else
                    {
                        var left = argument;
                        var right = parameterName;

                        if(parameter.Modifiers.ToString() == "ref")
                        {
                            (left, right) = (right, left);
                        }

                        var mappingContext = new MappingContext(
                                                   context,
                                                   apiNamespace,
                                                   statements,
                                                   left,
                                                   right,
                                                   generatedTypeSymbol,
                                                   Direction: MappingDirection.Out);

                        allocRequired = MemberMapper.MapMembers(mappingContext) || allocRequired;
                    }


                }
            }
            else if (typeName == "string")
            {
                argument = $"SilkMarshal.PtrToString((nint){argument}, NativeStringEncoding.UTF8)";
            }

            if (parameter.Modifiers is { Count: > 0 })
            {
                argument = $"{parameter.Modifiers.ToString()} {argument}";
            }

            arguments.Add(argument);
        }

        if (!allocated && allocRequired)
        {
            statements.InsertLine(insertAt, "using var m = new MarshalHelper();");
        }
    }



    public static bool MapMembers(MappingContext mappingContext)
    {
        var allocRequired = false;

        foreach (var member in mappingContext.MemberType.GetMembers().OfType<IFieldSymbol>())
        {
            var memberMappingContext = mappingContext with
            {
                Left = mappingContext.Left.EndsWith('>') ? $"{mappingContext.Left}{member.Name}" : $"{mappingContext.Left}.{member.Name}",
                Right = mappingContext.Right.EndsWith('>') ? $"{mappingContext.Right}{member.Name}" : $"{mappingContext.Right}.{member.Name}",
                MemberType = member.Type,
                MemberName = member.Name
            };

            allocRequired = MapMember(memberMappingContext) || allocRequired;
        }

        return allocRequired;
    }


    public static bool MapMember(MappingContext mappingContext)
    {
        var allocRequired = false;

        if (mappingContext.MemberType is IArrayTypeSymbol arrayType && arrayType.ElementType is INamedTypeSymbol et)
        {
            var memberType = et;

            var childLeft = mappingContext.Left;
            var childRight = mappingContext.Right;

            mappingContext.Statements.AppendLine($"if ({childRight} != null) {{");
            mappingContext.Statements.IncrementIndent();

            switch (mappingContext.Direction)
            {
                case MappingDirection.In:
                    {
                        var count = childLeft.Singularize() + "Count";
                        mappingContext.Statements.AppendLine($"{count} = (nuint){childRight}!.Length;");
                        var apiType = $"{mappingContext.ApiNamespace}.{memberType.Name}";
                        mappingContext.Statements.AppendLine($"{childLeft} = m.AllocatePtr<{apiType}>({childRight}.Length);");

                        mappingContext.Statements.AppendLine($"for (int {mappingContext.Index} = 0; {mappingContext.Index} < {childRight}!.Length; {mappingContext.Index}++)");
                        break;
                    }
                case MappingDirection.Out:
                    {
                        var count = childRight.Singularize() + "Count";
                        mappingContext.Statements.AppendLine($"{childLeft} = new {memberType.Name}[{count}];");

                        mappingContext.Statements.AppendLine($"for (int {mappingContext.Index} = 0; {mappingContext.Index} < {childLeft}!.Length; {mappingContext.Index}++)");

                        break;
                    }
            }

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
                memberExpression = mappingContext.Direction switch
                {
                    MappingDirection.In => $"({mappingContext.ApiNamespace}.{namedMemberType.Name}){memberExpression}",
                    MappingDirection.Out => $"({namedMemberType.Name}){memberExpression}",
                    _ => throw new NotSupportedException()
                };
                mappingContext.Statements.AppendLine($"{mappingContext.Left} = {memberExpression};");
            }
            else if (namedMemberType.SpecialType == SpecialType.System_String)
            {
                memberExpression = mappingContext.Direction switch
                {
                    MappingDirection.In => $"m.Utf8Ptr({memberExpression})",
                    MappingDirection.Out => $"SilkMarshal.PtrToString((nint){memberExpression}, NativeStringEncoding.UTF8)",
                    _ => throw new NotSupportedException()
                };

                mappingContext.Statements.AppendLine($"{mappingContext.Left} = {memberExpression};");
                allocRequired = mappingContext.Direction == MappingDirection.In;
            }
            else if (namedMemberType.TypeKind == TypeKind.Struct &&
                     mappingContext.Context.IsGeneratedTypeSymbol(namedMemberType) &&
                     mappingContext.Context.ShouldMarshall(namedMemberType.Name))
            {
                var childLeft = mappingContext.Left;
                var childRight = memberExpression;

                if (namedMemberType.IsNullableOfT(out var nullableType))
                {
                    switch (mappingContext.Direction)
                    {
                        case MappingDirection.In:
                            {
                                namedMemberType = nullableType;
                                var childMemberName = childLeft.Replace('.', '_').Replace('[', '_').Replace(']', '_');

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
                                break;
                            }
                        case MappingDirection.Out:
                            {
                                namedMemberType = nullableType;

                                mappingContext.Statements.AppendLine($"if ({childRight} != null) {{");
                                mappingContext.Statements.IncrementIndent();

                                var childMemberName = childLeft.Replace('.', '_').Replace('[', '_').Replace(']', '_');

                                mappingContext.Statements.AppendLine($"{namedMemberType.Name} {childMemberName} = default;");

                                childLeft = childMemberName;
                                childRight = $"{childRight}[0]";

                                var memberMappingContext = mappingContext with
                                {
                                    Left = childLeft,
                                    Right = childRight,
                                    MemberType = namedMemberType,
                                };

                                allocRequired = MapMembers(memberMappingContext) || allocRequired;

                                mappingContext.Statements.AppendLine($"{mappingContext.Left} = {childMemberName}; //PINREF");
                                mappingContext.Statements.DecrementIndent();
                                mappingContext.Statements.AppendLine("}");
                                break;
                            }
                    }



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
