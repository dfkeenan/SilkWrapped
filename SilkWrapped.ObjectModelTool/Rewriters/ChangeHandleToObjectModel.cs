using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;

namespace SilkWrapped.ObjectModelTool.Rewriters;
internal class ChangeHandleToObjectModel : ContextAwareCSharpSyntaxRewriter
{
    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        var semanticModel = Context.Compilation!.GetSemanticModel(node.SyntaxTree);
        var methods = node.Members.OfType<MethodDeclarationSyntax>();
        var changes = new Dictionary<MethodDeclarationSyntax, ITypeSymbol>();

        foreach ( var method in methods)
        {
            var methodSymbol = semanticModel?.GetDeclaredSymbol(method);
            
            if (methodSymbol?.ReturnType is ITypeSymbol { Name: string name } && name.EndsWith("Handle"))
            {
                if (Context.TryGetGeneratedTypeSymbol(name, out var typeSymbol))
                {
                    var propertyReference = FindReferenceSymbolsWithReturnType(typeSymbol, Context.Project.Solution)
                                            .Result.OfType<IPropertySymbol>().FirstOrDefault();

                    if (propertyReference != null)
                    {
                        changes[method] = propertyReference.ContainingType;
                    }
                }
            }
        }

        if(changes.Count > 0)
        {
            node = node.ReplaceNodes(changes.Keys, (old, updated) =>
            {
                return updated.WithReturnType(TypeSyntax(changes[old]));
            });

            return node;
        }

        return base.VisitClassDeclaration(node);
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
