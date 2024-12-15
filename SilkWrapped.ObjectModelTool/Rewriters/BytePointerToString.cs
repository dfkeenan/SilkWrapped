using System.Text.Json.Serialization;

namespace SilkWrapped.ObjectModelTool.Rewriters;

internal class BytePointerToString : CSharpSyntaxRewriter
{
    private readonly HashSet<string> excludeNames;
    private readonly TypeSyntax stringSyntax = ParseTypeName("string? ");

    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public ISet<string> ExcludeNames => excludeNames;
    public BytePointerToString()
    {
        excludeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public BytePointerToString(params string[] names)
    {
        this.excludeNames = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
    }

    public override SyntaxNode? VisitVariableDeclaration(VariableDeclarationSyntax node)
    {
        if (node.Variables is [var variable] && ShouldChange(variable.Identifier.Text) && node.Type is PointerTypeSyntax pointerType &&
            pointerType.ElementType.ToString() == "byte")
        {
            return node.WithType(stringSyntax);
        }

        return base.VisitVariableDeclaration(node);
    }

    public override SyntaxNode? VisitParameter(ParameterSyntax node)
    {
        if (ShouldChange(node.Identifier.Text) && node.Type is PointerTypeSyntax pointerType &&
            pointerType.ElementType.ToString() == "byte")
        {
            return node.WithType(stringSyntax);
        }

        return base.VisitParameter(node);
    }

    private bool ShouldChange(string name) 
        => excludeNames is {Count: 0 } || !excludeNames.Contains(name);
}
