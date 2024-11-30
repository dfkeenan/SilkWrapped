using SilkWrapped.SourceGenerator;

namespace SilkWrapped.ObjectModelTool.Rewriters;

internal class AddObjectModelConstructor : ContextAwareCSharpSyntaxRewriter
{
    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        var properties = node.Members.OfType<PropertyDeclarationSyntax>();

        if (properties is null || !properties.Any())
        {
            return base.VisitClassDeclaration(node);
        }

        var propertiesArray = properties.ToArray();
        node = node.InsertConstructor(propertiesArray.Length, propertiesArray);

        return node;
    }
}
