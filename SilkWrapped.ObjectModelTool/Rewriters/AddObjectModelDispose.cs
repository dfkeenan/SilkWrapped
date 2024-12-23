using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using SilkWrapped.SourceGenerator.Common;

namespace SilkWrapped.ObjectModelTool.Rewriters;

internal class AddObjectModelDispose : ContextAwareCSharpSyntaxRewriter
{

    public const string DefaultDisposalMethodNamePattern = ".*(Release|Drop|Destroy).*";
    public required string DisposalMethodNamePattern { get; set; } = DefaultDisposalMethodNamePattern;

    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public List<string> Priority { get; } = [];

    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {

        var handleType = node.Members.OfType<PropertyDeclarationSyntax>().FirstOrDefault(p => p.Identifier.Text == "Handle")?.Type;

        if (handleType is null) return node;

        var disposalMethods = (from member in node.Members.OfType<MethodDeclarationSyntax>()
                               let match = Regex.Match(member.Identifier.Text, DisposalMethodNamePattern)
                               where match.Success
                               select (member, match.Groups[1].Value)).ToLookup(i => i.Value, i => i.member);

        if (disposalMethods.Count == 0) return node;

        var disposalMethod = Priority.SelectMany(p => disposalMethods[p]).FirstOrDefault();
        if (disposalMethod is null) return node;

        var disposeMethodStatements = new SyntaxList<StatementSyntax>();
        disposeMethodStatements = disposeMethodStatements.Add(ParseStatement("if (Handle.IsEmpty) return;"));
        disposeMethodStatements = disposeMethodStatements.Add(ParseStatement("Disposing();"));

        string statement = $"{disposalMethod.Identifier.Text}();";
        disposeMethodStatements = disposeMethodStatements.Add(ParseStatement(statement));

        if (TypeName(handleType) == Context.ApiOwnerTypeSymbol.Name)
        {
            disposeMethodStatements = disposeMethodStatements.Add(ParseStatement($"{Context.ApiName}.Dispose();"));
        }

        disposeMethodStatements = disposeMethodStatements.Add(ParseStatement("Handle = default;"));
        disposeMethodStatements = disposeMethodStatements.Add(ParseStatement("Disposed();"));
        node = node.AddDispose(b => b.AddStatements(disposeMethodStatements))
                                 .AddMembers(ParseMemberDeclaration($"    partial void Disposing();{Environment.NewLine}")!,
                                 ParseMemberDeclaration($"    partial void Disposed();{Environment.NewLine}")!);

        return node;
    }
}
