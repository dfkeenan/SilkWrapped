using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;
using SilkWrapped.ObjectModelTool.Rewriters;

namespace SilkWrapped.ObjectModelTool.GeneratorTransforms;
internal class ExtractTypes : GeneratorTransformBase
{
    [JsonConverter(typeof(JsonStringEnumConverter<TypeKind>))]
    public TypeKind TypeKind { get; set; }
    public string? ExcludeTypesPattern { get; set; }
    public string? IncludeTypesPattern { get; set; }
    public string? OutputPath { get; set; }

    public bool SkipMarhsalling { get; set; }

    public List<CSharpSyntaxRewriter> Rewriters { get; set; } = [];

    public override async Task TransformAsync(GeneratorTransformContext context, CancellationToken cancellationToken)
    {
        if (TypeKind == TypeKind.Unknown) return;
        if (context.Decompiler is not Decompiler decompiler) return;

        INamespaceSymbol containingNamespace = context.ApiTypeSymbol.ContainingNamespace;
        var typeSymbols
            = containingNamespace.GetMembers()
                .OfType<INamedTypeSymbol>()
                .Where(ts => ts.TypeKind == TypeKind);

        if (!string.IsNullOrEmpty(ExcludeTypesPattern))
        {
            var regex = new Regex(ExcludeTypesPattern, RegexOptions.Compiled);

            typeSymbols = typeSymbols.Where(ts => !regex.IsMatch(ts.Name));
        }

        if (!string.IsNullOrEmpty(IncludeTypesPattern))
        {
            var regex = new Regex(IncludeTypesPattern, RegexOptions.Compiled);

            typeSymbols = typeSymbols.Where(ts => regex.IsMatch(ts.Name));
        }

        var namespaceReplacer
            = new ReplaceNamespace(containingNamespace.ToDisplayString(), context.Project.DefaultNamespace!);

        foreach (var typeSymbol in typeSymbols)
        {
            if (context.Items.Any(i => TypeName(i.SourceType) == typeSymbol.Name)) continue;

            var typeSyntax = context.Decompiler.GetSyntax(typeSymbol);
            if (typeSyntax is null) continue;
            typeSyntax = namespaceReplacer.Visit(typeSyntax);

            var type = ParseTypeName(typeSymbol.Name);

            var item = await context.AddItem(OutputPath, typeSymbol.Name, typeSyntax, type, type!, cancellationToken);

            if (SkipMarhsalling)
            {
                context.SkipMarhsalling(typeSymbol.Name);
            }

            foreach (var rewriter in Rewriters)
            {
                typeSyntax = await context.GetSyntaxRootAsync(item.DocumentId, cancellationToken);

                if (typeSyntax is null) continue;

                typeSyntax = rewriter switch
                {
                    ContextAwareCSharpSyntaxRewriter contextRewriter => contextRewriter.Visit(typeSyntax, context),
                    _ => rewriter.Visit(typeSyntax)
                };

                await context.UpdateDocumentAsync(item.DocumentId, typeSyntax, cancellationToken);
            }

            
        }

    }

    public override string ToString()
    {
        return $"{GetType().Name} - {TypeKind}";
    }
}
