using Humanizer;
using SilkWrapped.SourceGenerator.Common;

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
        MappingDirection Direction = MappingDirection.In,
        bool InLoop = false);

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
                argument = $"m.RentUtf8Ptr({argument})";
                allocRequired = true;
            }

            if (parameter.Modifiers is { Count: > 0 })
            {
                argument = $"{parameter.Modifiers} {argument}";
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

                        if (parameter.Modifiers.ToString() == "ref")
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
                argument = $"{parameter.Modifiers} {argument}";
            }

            arguments.Add(argument);
        }

        if (!allocated && allocRequired)
        {
            statements.InsertLine(insertAt, "using var m = new MarshalHelper();");
        }
    }



    public static bool MapMembers(MappingContext context)
    {
        var allocRequired = false;

        if (context.MemberType is INamedTypeSymbol namedTypeSymbol &&
            context.Context.IsBlittable(namedTypeSymbol) &&
            context.Context.TryGetApiTypeSymbol(namedTypeSymbol.Name, out var apiTypeSymbol))
        {
            var fromType = namedTypeSymbol.Name;
            var toType = apiTypeSymbol.ToDisplayString();

            if (context.Direction == MappingDirection.Out)
            {
                (fromType, toType) = (toType, fromType);
            }

            context.Statements.AppendLine($"{context.Left} = Unsafe.BitCast<{fromType}, {toType}>({context.Right});");

        }
        else
        {
            foreach (var member in context.MemberType.GetMembers().OfType<IFieldSymbol>())
            {
                var memberMappingContext = context with
                {
                    Left = context.Left.EndsWith('>') ? $"{context.Left}{member.Name}" : $"{context.Left}.{member.Name}",
                    Right = context.Right.EndsWith('>') ? $"{context.Right}{member.Name}" : $"{context.Right}.{member.Name}",
                    MemberType = member.Type,
                    MemberName = member.Name
                };

                allocRequired = MapMember(memberMappingContext) || allocRequired;
            }
        }

        return allocRequired;
    }


    public static bool MapMember(MappingContext context)
    {
        var allocRequired = false;

        if (context.MemberType is IArrayTypeSymbol arrayType && arrayType.ElementType is INamedTypeSymbol et)
        {
            var memberType = et;

            var childLeft = context.Left;
            var childRight = context.Right;

            context.Statements.AppendLine($"if ({childRight} != null) {{");
            context.Statements.IncrementIndent();

            if (context.Direction == MappingDirection.In && context.Context.IsBlittable(et))
            {
                var count = childLeft.Singularize() + "Count";
                context.Statements.AppendLine($"{count} = (nuint){childRight}!.Length;");

                if (context.Context.IsHandleType(memberType, out var apiHandleType))
                {
                    var apiType = $"{context.ApiNamespace}.{apiHandleType.Name}";

                    childRight = $"({apiType}**)m.Pin<{memberType.Name}, {memberType.Name}>({childRight})";
                }
                else
                {
                    var apiType = $"{context.ApiNamespace}.{memberType.Name}";

                    childRight = $"m.Pin<{memberType.Name}, {apiType}>({childRight})";
                }

                context.Statements.AppendLine($"{childLeft} = {childRight};");
            }
            else
            {
                switch (context.Direction)
                {
                    case MappingDirection.In:
                        {
                            var count = childLeft.Singularize() + "Count";
                            context.Statements.AppendLine($"{count} = (nuint){childRight}!.Length;");
                            var apiType = $"{context.ApiNamespace}.{memberType.Name}";
                            context.Statements.AppendLine($"{childLeft} = MarshalHelper.AsPointer({childRight}!.Length <= MarshalHelper.MaxStack ? stackalloc {apiType}[{childRight}!.Length] : m.RentSpan<{apiType}>({childRight}!.Length));");
                            //context.Statements.AppendLine($"{childLeft} = m.RentPtr<{apiType}>({childRight}.Length);");

                            context.Statements.AppendLine($"for (int {context.Index} = 0; {context.Index} < {childRight}!.Length; {context.Index}++)");
                            break;
                        }
                    case MappingDirection.Out:
                        {
                            var count = childRight.Singularize() + "Count";
                            context.Statements.AppendLine($"{childLeft} = new {memberType.Name}[{count}];");

                            context.Statements.AppendLine($"for (int {context.Index} = 0; {context.Index} < {childLeft}!.Length; {context.Index}++)");

                            break;
                        }
                }

                context.Statements.AppendLine("{");
                context.Statements.IncrementIndent();
                childLeft = $"{childLeft}[{context.Index}]";
                childRight = $"{childRight}![{context.Index}]";

                var arrayMappingContext = context with
                {
                    Left = childLeft,
                    Right = childRight,
                    MemberType = memberType,
                    MemberName = "",
                    Index = (char)(context.Index + 1),
                    InLoop = true,
                };

                allocRequired = MapMember(arrayMappingContext) || allocRequired;

                context.Statements.DecrementIndent();
                context.Statements.AppendLine("}");
            }

            context.Statements.DecrementIndent();
            context.Statements.AppendLine("}");
            allocRequired = true;

        }
        else if (context.MemberType is INamedTypeSymbol namedMemberType)
        {
            var memberExpression = context.Right;

            if (namedMemberType.TypeKind == TypeKind.Enum)
            {
                memberExpression = context.Direction switch
                {
                    MappingDirection.In => $"({context.ApiNamespace}.{namedMemberType.Name}){memberExpression}",
                    MappingDirection.Out => $"({namedMemberType.Name}){memberExpression}",
                    _ => throw new NotSupportedException()
                };
                context.Statements.AppendLine($"{context.Left} = {memberExpression};");
            }
            else if (namedMemberType.SpecialType == SpecialType.System_String)
            {
                memberExpression = context.Direction switch
                {
                    MappingDirection.In => $"m.RentUtf8Ptr({memberExpression})",
                    MappingDirection.Out => $"SilkMarshal.PtrToString((nint){memberExpression}, NativeStringEncoding.UTF8)",
                    _ => throw new NotSupportedException()
                };

                context.Statements.AppendLine($"{context.Left} = {memberExpression};");
                allocRequired = context.Direction == MappingDirection.In;
            }
            else if (namedMemberType.TypeKind == TypeKind.Struct &&
                     context.Context.IsGeneratedTypeSymbol(namedMemberType) &&
                     context.Context.ShouldMarshall(namedMemberType.Name))
            {
                var childLeft = context.Left;
                var childRight = memberExpression;

                if (namedMemberType.IsNullableOfT(out var nullableType))
                {
                    switch (context.Direction)
                    {
                        case MappingDirection.In:
                            {
                                namedMemberType = nullableType;
                                var childMemberName = childLeft.Replace('.', '_').Replace('[', '_').Replace(']', '_');

                                context.Statements.AppendLine($"{context.ApiNamespace}.{namedMemberType.Name} {childMemberName} = default;");
                                context.Statements.AppendLine($"if ({childRight}.HasValue) {{");
                                context.Statements.IncrementIndent();
                                childLeft = childMemberName;
                                childRight = $"{childRight}!.Value";

                                var memberMappingContext = context with
                                {
                                    Left = childLeft,
                                    Right = childRight,
                                    MemberType = namedMemberType,
                                };

                                allocRequired = MapMembers(memberMappingContext) || allocRequired;

                                if (context.InLoop)
                                {
                                    context.Statements.AppendLine($"{context.Left} = m.RentPtr(ref {childMemberName}); //PINREF");
                                    allocRequired = true;
                                }
                                else
                                {
                                    context.Statements.AppendLine($"{context.Left} = &{childMemberName}; //PINREF");
                                }
                                context.Statements.DecrementIndent();
                                context.Statements.AppendLine("}");
                                break;
                            }
                        case MappingDirection.Out:
                            {
                                namedMemberType = nullableType;

                                context.Statements.AppendLine($"if ({childRight} != null) {{");
                                context.Statements.IncrementIndent();

                                var childMemberName = childLeft.Replace('.', '_').Replace('[', '_').Replace(']', '_');

                                context.Statements.AppendLine($"{namedMemberType.Name} {childMemberName} = default;");

                                childLeft = childMemberName;
                                childRight = $"{childRight}[0]";

                                var memberMappingContext = context with
                                {
                                    Left = childLeft,
                                    Right = childRight,
                                    MemberType = namedMemberType,
                                };

                                allocRequired = MapMembers(memberMappingContext) || allocRequired;

                                context.Statements.AppendLine($"{context.Left} = {childMemberName}; //PINREF");
                                context.Statements.DecrementIndent();
                                context.Statements.AppendLine("}");
                                break;
                            }
                    }



                }
                else if (context.Context.IsHandleType(namedMemberType, out _))
                {
                    context.Statements.AppendLine($"{childLeft} = {childRight};");
                }
                else
                {
                    allocRequired = MapMembers(context) || allocRequired;
                }
            }
            else
            {
                context.Statements.AppendLine($"{context.Left} = {memberExpression};");
            }
        }
        return allocRequired;
    }
}
