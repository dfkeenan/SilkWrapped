using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis.CSharp;

namespace SilkWrapped.ObjectModelTool.Rewriters;

internal class BytePointerToString : CSharpSyntaxRewriter
{
    private readonly HashSet<string> names;
    private readonly TypeSyntax stringSyntax = ParseTypeName("string? ");

    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public ISet<string> Names => names;
    public BytePointerToString()
    {
        names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public BytePointerToString(params string[] names)
    {
        this.names = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
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
        => names is {Count: 0 } || names.Contains(name);
}
