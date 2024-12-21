using System.Text.Json.Serialization;

namespace SilkWrapped.ObjectModelTool.Rewriters;
internal class RemoveAttributes : CSharpSyntaxRewriter
{
    private readonly HashSet<string> names;

    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public ISet<string> Names => names;

    public RemoveAttributes(params string[] names)
    {
        this.names = [.. names];
    }

    public RemoveAttributes()
    {
        names = [];
    }

    public override SyntaxNode? VisitAttribute(AttributeSyntax node)
    {
        if (names.Count == 0 || names.Contains(node.Name.ToString()))
        {
            return null;
        }

        return base.VisitAttribute(node);
    }

    public override SyntaxNode? VisitAttributeList(AttributeListSyntax node)
    {
        var visitedNode = base.VisitAttributeList(node) as AttributeListSyntax;

        if (visitedNode?.Attributes is null or [])
        {
            return null;
        }

        return visitedNode;
    }
}
