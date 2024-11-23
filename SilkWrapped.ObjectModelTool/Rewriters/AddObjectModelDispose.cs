using System.Text.RegularExpressions;

namespace SilkWrapped.ObjectModelTool.Rewriters;

internal class AddObjectModelDispose : ContextAwareCSharpSyntaxRewriter
{
    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {

        var handleType = node.Members.OfType<PropertyDeclarationSyntax>().FirstOrDefault(p => p.Identifier.Text == "Handle")?.Type;

        if (handleType is null) return node;

        var disposalMethods = node.Members.OfType<MethodDeclarationSyntax>()
                                            .Where(m => Regex.IsMatch(m.Identifier.Text, Context.Generator.DisposalMethodNamePattern))
                                            .ToList();

        if (disposalMethods.Count == 0) return node;


        var disposeMethodStatements = new SyntaxList<StatementSyntax>();
        disposeMethodStatements = disposeMethodStatements.Add(ParseStatement("if (Handle == default) return;"));
        disposeMethodStatements = disposeMethodStatements.Add(ParseStatement("Disposing();"));

        foreach (var disposalMethod in disposalMethods)
        {
            string statement = $"Api.{TypeName(handleType)}{disposalMethod.Identifier.Text}(Handle);";
            disposeMethodStatements = disposeMethodStatements.Add(ParseStatement(statement));
        }

        if (TypeName(handleType) == Context.ApiOwnerTypeSymbol.Name)
        {
            disposeMethodStatements = disposeMethodStatements.Add(ParseStatement("Api.Dispose();"));
        }

        disposeMethodStatements = disposeMethodStatements.Add(ParseStatement("Disposed();"));
        node = node.AddDispose(b => b.AddStatements(disposeMethodStatements))
                                 .AddMembers(ParseMemberDeclaration($"    partial void Disposing();{Environment.NewLine}")!,
                                 ParseMemberDeclaration($"    partial void Disposed();{Environment.NewLine}")!);

        return node;
    }
}
