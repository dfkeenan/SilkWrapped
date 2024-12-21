namespace SilkWrapped.ObjectModelTool.Rewriters;
internal class MakeStructPartial : CSharpSyntaxRewriter
{
    private readonly SyntaxToken partialToken = ParseToken("partial ");

    public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node)
    {
        node = node.AddModifiers(partialToken);

        return base.VisitStructDeclaration(node);
    }
}
