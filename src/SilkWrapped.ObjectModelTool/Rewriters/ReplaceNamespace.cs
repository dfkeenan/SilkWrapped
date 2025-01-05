namespace SilkWrapped.ObjectModelTool.Rewriters;
internal class ReplaceNamespace(string from, string to) : CSharpSyntaxRewriter
{
    private NameSyntax toName = ParseName(to);
    public override SyntaxNode? VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        if (node.Name.ToFullString() == from)
        {
            return node.WithName(toName);
        }

        return base.VisitNamespaceDeclaration(node);
    }

    public override SyntaxNode? VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
    {
        if (node.Name.ToFullString() == from)
        {
            return node.WithName(toName);
        }

        return base.VisitFileScopedNamespaceDeclaration(node);
    }
}
