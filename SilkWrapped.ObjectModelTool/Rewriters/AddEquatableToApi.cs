namespace SilkWrapped.ObjectModelTool.Rewriters;
internal class AddEquatableToApi : ContextAwareCSharpSyntaxRewriter
{

    public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node)
    {
        if (Context.TryGetGeneratedTypeSymbol(node.Identifier.Text, out var typeSymbol) &&
            Context.IsHandleType(typeSymbol, out _))
        {
            node = node.AddBaseListTypes(SimpleBaseType(ParseTypeName($" IEquatable<{node.Identifier.Text}>")));

            node = node.AddMembers(
                ParseMemberDeclaration($"public static bool operator ==({node.Identifier.Text} handle, {node.Identifier.Text} other) => handle.nativeHandle == other.nativeHandle;")!,
                ParseMemberDeclaration($"public static bool operator !=({node.Identifier.Text} handle, {node.Identifier.Text} other) => handle.nativeHandle != other.nativeHandle;")!,
                ParseMemberDeclaration($"public bool Equals({node.Identifier.Text} other) => this == other;")!,
                ParseMemberDeclaration($$""" 

                                        public override bool Equals([NotNullWhen(true)] object? obj)
                                        {
                                            if (obj is not {{node.Identifier.Text}} other) return false;
                                            return this == other;
                                        }
                                        """)!,
                ParseMemberDeclaration($"public override int GetHashCode() => (int)nativeHandle;")!);
        }

        return base.VisitStructDeclaration(node);
    }

    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (Context.TryGetHandleTypeName(node.Identifier.Text, out _))
        {
            node = node.AddBaseListTypes(SimpleBaseType(ParseTypeName($" IEquatable<{node.Identifier.Text}>")));

            node = node.AddMembers(
                ParseMemberDeclaration($$""" 

                                        public bool Equals([NotNullWhen(true)] {{node.Identifier.Text}}? other)
                                        {
                                            if (other is null) return false;
                                            return Handle == other.Handle;
                                        }
                                        """)!,
                ParseMemberDeclaration($$""" 

                                        public override bool Equals([NotNullWhen(true)] object? obj)
                                        {
                                            if (obj is not {{node.Identifier.Text}} other) return false;
                                            return Handle == other.Handle;
                                        }
                                        """)!,
                ParseMemberDeclaration($"public override int GetHashCode() => Handle.GetHashCode();")!);

        }
        return base.VisitClassDeclaration(node);
    }
}
