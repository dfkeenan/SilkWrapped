namespace SilkWrapped.ObjectModelTool.Rewriters;
internal class PrettyParameterName : ContextAwareCSharpSyntaxRewriter
{
    public override SyntaxNode? VisitParameter(ParameterSyntax node)
    {

        var typeName = TypeName(node.Type);

        var name = typeName switch
        {
            "string" => "message",
            "void" => "data",
            var s when s.EndsWith("Status") => "status",
            var s when s.EndsWith("Handle") => char.ToLowerInvariant(s[0]) + s.Substring(1, s.Length - "Handle".Length - 1),
            var s => ToPascalCase(s!)
        };

        node = node.WithIdentifier(Identifier(name));

        return base.VisitParameter(node);
    }
}
