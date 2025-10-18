using System.Text.Json.Serialization;

namespace SilkWrapped.ObjectModelTool.Rewriters;

internal class AddUsingDirectivies : CSharpSyntaxRewriter
{
    private readonly HashSet<string> names;

    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public ISet<string> Names => names;

    public AddUsingDirectivies(params string[] names)
    {
        this.names = [.. names];
    }

    public AddUsingDirectivies()
    {
        names = [];
    }

    public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node)
    {

        var usings = node.Usings;

        foreach (var name in names)
        {
            if (node.Usings.Any(us => us.Name?.ToString() is string n && n == name)) continue;

            usings = usings.Add(UsingDirective(ParseName(name)));
        }

        return node.WithUsings(usings);
    }
}