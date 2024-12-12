


namespace SilkWrapped.ObjectModelTool.Rewriters;
internal class MakeNullableRef : CSharpSyntaxRewriter
{
    private Location location;

    public MakeNullableRef(Location location)
    {
        this.location = location;
    }

    public override SyntaxNode? VisitParameter(ParameterSyntax node)
    {
        if (node.Span.IntersectsWith(location.SourceSpan) &&
            node.Type is NullableTypeSyntax nts)
        {
            node = node.WithType(ParseTypeName($"NullableRef<{nts.ElementType.ToString()}>"));
        }

        return base.VisitParameter(node);
    }

    public override SyntaxNode? VisitVariableDeclaration(VariableDeclarationSyntax node)
    {
        if (node.Span.IntersectsWith(location.SourceSpan) &&
            node.Type is NullableTypeSyntax nts)
        {
            node = node.WithType(ParseTypeName($"NullableRef<{nts.ElementType.ToString()}>"));
        }


        return base.VisitVariableDeclaration(node);
    }
}
