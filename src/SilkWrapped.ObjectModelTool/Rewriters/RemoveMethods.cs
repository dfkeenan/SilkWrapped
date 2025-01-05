using System.Text.Json.Serialization;

namespace SilkWrapped.ObjectModelTool.Rewriters;
internal class RemoveMethods : CSharpSyntaxRewriter
{
    private readonly HashSet<string> names;
    private readonly TypeSyntax stringSyntax = ParseTypeName("string? ");

    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public ISet<string> Names => names;
    public RemoveMethods()
    {
        names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public RemoveMethods(params string[] names)
    {
        this.names = new HashSet<string>(names);
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (names.Contains(node.Identifier.Text)) return null;

        return base.VisitMethodDeclaration(node);
    }
}
