using System.Linq;
using System.Text.Json.Serialization;

namespace SilkWrapped.ObjectModelTool.Rewriters;

internal class RemoveUsingDirectivies : CSharpSyntaxRewriter
{
    private readonly HashSet<string> names;

    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public ISet<string> Names => names;

    public RemoveUsingDirectivies(params string[] names)
    {
        this.names = [.. names];
    }

    public RemoveUsingDirectivies()
    {
        names = [];
    }

    public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node)
    {

        var usings = node.Usings;

        foreach (var us in usings) 
        {
            if (us.Name?.ToString() is string name && names.Contains(name))
            {
                usings = usings.Remove(us);

            }
        }

        return node.WithUsings(usings);

    }
}