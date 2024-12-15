using Microsoft.CodeAnalysis.FindSymbols;

namespace SilkWrapped.ObjectModelTool.Rewriters;
internal class ChangeHandleToObjectModel : ContextAwareCSharpSyntaxRewriter
{
    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (TypeName(node.ReturnType) is string returnTypeName && 
            Context.TryGetObjectTypeName(returnTypeName, out var objectTypeName))
        {
            var statements = node.Body!.Statements;
            if (statements[statements.Count - 1] is ReturnStatementSyntax returnStatement)
            {
                var expression = ParseExpression($"new {objectTypeName}({Context.ApiName}, {returnStatement.Expression})");
                returnStatement = returnStatement.WithExpression(expression);
                statements = statements.RemoveAt(statements.Count - 1).Add(returnStatement);

                node = node.WithBody(Block().WithStatements(statements));
            }
            return node.WithReturnType(ParseTypeName(objectTypeName).WithTriviaFrom(node.ReturnType));
        }


        return base.VisitMethodDeclaration(node);
    }

    protected static async Task<IEnumerable<ISymbol>> FindReferenceSymbolsWithReturnType(ITypeSymbol typeSymbol, Solution solution)
    {
        var references = await SymbolFinder.FindReferencesAsync(typeSymbol, solution);
        var referenceLocations = references?.SelectMany(r => r.Locations);

        if (referenceLocations is null) return Enumerable.Empty<ISymbol>();

        var result = new List<ISymbol>();

        var referenceGroups = referenceLocations.GroupBy(r => r.Document, r=> r.Location);

        foreach (var group in referenceGroups)
        {
            var document = group.Key;
            var semanticModel = await document.GetSemanticModelAsync();
            if (semanticModel is null) continue;
            var root = await document.GetSyntaxRootAsync();
            if (root is null) continue;

            foreach (var location in group)
            {
                var symbol = semanticModel.GetEnclosingSymbol(location.SourceSpan.Start);

                var syntaxNode = root.FindNode(location.SourceSpan);

                var memberSyntax = syntaxNode.Ancestors().OfType<MemberDeclarationSyntax>().FirstOrDefault();

                symbol = memberSyntax switch
                {
                    PropertyDeclarationSyntax propertyDeclaration 
                        when semanticModel.GetDeclaredSymbol(propertyDeclaration) is IPropertySymbol propertySymbol && 
                             SymbolEqualityComparer.Default.Equals(propertySymbol.Type, typeSymbol) => propertySymbol,
                    FieldDeclarationSyntax fieldDeclaration
                        when semanticModel.GetDeclaredSymbol(fieldDeclaration) is IFieldSymbol fieldSymbol &&
                             SymbolEqualityComparer.Default.Equals(fieldSymbol.Type, typeSymbol) => fieldSymbol,
                    MethodDeclarationSyntax methodDeclaration
                        when semanticModel.GetDeclaredSymbol(methodDeclaration) is IMethodSymbol methodSymbol &&
                             SymbolEqualityComparer.Default.Equals(methodSymbol.ReturnType, typeSymbol) => methodSymbol,
                    _ => null
                };

                if (symbol is not null)
                {
                    result.Add(symbol);
                }
            }
        }

        return result;
    }
}
