namespace SilkWrapped.ObjectModelTool.Rewriters;
internal class PrettyParameterName : ContextAwareCSharpSyntaxRewriter
{
    public override SyntaxNode? VisitParameter(ParameterSyntax node)
    {

        var typeName = TypeName(node.Type);

        if (typeName is not null)
        {
            var name = typeName switch
            {
                "string" => "message",
                "void" => "data",
                string s when s.EndsWith("Status") => "status",
                string s when s.EndsWith("Handle") => char.ToLowerInvariant(s[0]) + s[1..^"Handle".Length],
                string s => ToPascalCase(s!),
            };

            node = node.WithIdentifier(Identifier(name));
        }

        return base.VisitParameter(node);
    }
}
