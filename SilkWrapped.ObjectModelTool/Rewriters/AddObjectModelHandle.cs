namespace SilkWrapped.ObjectModelTool.Rewriters;
internal class AddObjectModelHandle : ContextAwareCSharpSyntaxRewriter
{
    private MemberDeclarationSyntax? handleDeclaratation;

    public override SyntaxNode? VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
    {
        node = (FileScopedNamespaceDeclarationSyntax)base.VisitFileScopedNamespaceDeclaration(node)!;

        if (handleDeclaratation != null)
        {
            node = node.WithMembers(node.Members.Insert(0, handleDeclaratation));
        }

        return node;
    }

    public override SyntaxNode? VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        node = (NamespaceDeclarationSyntax)base.VisitNamespaceDeclaration(node)!;

        if (handleDeclaratation != null)
        {
            node = node.WithMembers(node.Members.Insert(0, handleDeclaratation));
        }

        return node;
    }

    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        var firstParameter = node.Members.OfType<MethodDeclarationSyntax>().FirstOrDefault()?.ParameterList.Parameters[0];

        if (firstParameter is null)
        {
            return base.VisitClassDeclaration(node);
        }

        var apiHandleType = ParseTypeName($"{Context.ApiTypeSymbol.ContainingNamespace.ToDisplayString()}.{firstParameter.Type!.ToString()}");

        var handleType = MakeHandleStruct(apiHandleType);

        Context.AddHandle(TypeName(handleType)!, node.Identifier.Text);

        var handleProperty = PropertyDeclaration(handleType, "Handle", SyntaxKind.PublicKeyword).WithSetter(SyntaxKind.PrivateKeyword);

        var members = node.Members.Insert(0, handleProperty);

        var castOperator = ParseMemberDeclaration($"public static implicit operator {handleType.ToString()}({node.Identifier.Text} obj) => obj.Handle;")!;
        members = members.Add(castOperator);

        node = node.WithMembers(members);

        return node;
    }

    private TypeSyntax MakeHandleStruct(TypeSyntax typeSyntax)
    {

        var handleTypeName = $"{TypeName(typeSyntax)}Handle";
        var nativeType = typeSyntax.ToString();

        var code = $$"""

                     public unsafe readonly struct {{handleTypeName}}
                     {
                        private readonly {{nativeType}} nativeHandle;

                        private {{handleTypeName}}({{nativeType}} nativeHandle)
                        {
                            this.nativeHandle = nativeHandle;
                        }

                        public bool IsEmpty => nativeHandle == default;

                        public static implicit operator {{nativeType}}({{handleTypeName}} handle) => handle.nativeHandle;
                        public static implicit operator {{handleTypeName}}({{nativeType}} handle) => new {{handleTypeName}}(handle);

                     }

                     """;

        handleDeclaratation = ParseMemberDeclaration(code)!;

        return ParseTypeName(handleTypeName);
    }
}
