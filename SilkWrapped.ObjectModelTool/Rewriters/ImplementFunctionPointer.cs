namespace SilkWrapped.ObjectModelTool.Rewriters;
internal class ImplementFunctionPointer : ContextAwareCSharpSyntaxRewriter
{
    public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node)
    {
        var toRemove = new List<SyntaxNode>();
        var delegateName = "";
        foreach (var item in node.Members)
        {
            switch (item)
            {
                case FieldDeclarationSyntax fieldDeclarationSyntax:
                    toRemove.Add(fieldDeclarationSyntax);
                    break;
                case PropertyDeclarationSyntax propertyDeclarationSyntax: 
                    toRemove.Add(propertyDeclarationSyntax); 
                    break;
                case ConversionOperatorDeclarationSyntax operatorDeclarationSyntax:
                    toRemove.Add(operatorDeclarationSyntax);
                    break;
                case ConstructorDeclarationSyntax {ParameterList.Parameters: [ParameterSyntax{Type: FunctionPointerTypeSyntax } ,..] } constructorDeclarationSyntax:
                    toRemove.Add(constructorDeclarationSyntax);
                    break;
                case MethodDeclarationSyntax {Identifier.Text: "From" } methodDeclarationSyntax:
                    delegateName = TypeName(methodDeclarationSyntax.ParameterList.Parameters[0].Type);
                    break;
                default:
                    break;
            }
        }

        node = node.RemoveNodes(toRemove, SyntaxRemoveOptions.KeepNoTrivia)!;

        var members = node.Members;

        var apiType = ParseTypeName($"{Context.ApiTypeSymbol.ContainingNamespace.ToDisplayString()}.{node.Identifier.Text}");

        members = members.Insert(0, ParseMemberDeclaration($"private readonly {apiType} callback;")!);
        members = members.Add(ParseMemberDeclaration($"public static implicit operator {apiType}({node.Identifier.Text} callback) => callback.callback;")!);
        //members = members.Add(ParseMemberDeclaration($"public static implicit operator {node.Identifier.Text}({delegateName} proc) => new {node.Identifier.Text}(proc);")!);


        node = node.WithMembers(members);


        return base.VisitStructDeclaration(node);
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if(node.Identifier.Text == nameof(IDisposable.Dispose))
        {
            node = node.WithBody(null)
                       .WithExpressionBody(ArrowExpressionClause(ParseExpression("callback.Dispose()")))
                       .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        return base.VisitMethodDeclaration(node);
    }

    public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {

        if (TypeName(node.ParameterList.Parameters[0].Type) is string typeName &&
            Context.TryGetGeneratedTypeSymbol(typeName, out var apiType) &&
            apiType.DelegateInvokeMethod is IMethodSymbol delegateType)
        {
            

            var parameters = delegateType.DeclaringSyntaxReferences.FirstOrDefault()?
                                           .SyntaxTree
                                           .GetRoot()
                                           .DescendantNodes()
                                           .OfType<ParameterListSyntax>()
                                           .FirstOrDefault()?
                                           .Parameters;

            if (parameters != null)
            {

                var arguments = new List<string>();
                var statements = new IndentedStringBuilder();

                statements.Append("callback = new((")
                          .AppendJoin(delegateType.Parameters.Select(p => p.Name))
                          .AppendLine(") =>")
                          .AppendLine("{")
                          .IncrementIndent();

                MemberMapper.MapOutParameters(
                    Context,
                    statements,
                    parameters,
                    arguments);

                statements.Append("proc(")
                          .AppendJoin(arguments)
                          .AppendLine(");")
                          .DecrementIndent()
                          .AppendLine("});");

                node = node.WithBody(Block(ParseStatement(statements.ToString())));
            }
        }

        

        return base.VisitConstructorDeclaration(node);
    }
}
