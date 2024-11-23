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
    public string? OutputPath { get; set; }

    public List<CSharpSyntaxRewriter> Rewriters { get; set; } = [];

    public override Task TransformAsync(GeneratorTransformContext context, CancellationToken cancellationToken)
    {
        if (TypeKind == TypeKind.Unknown) return Task.CompletedTask;
        if (context.Decompiler is not Decompiler decompiler) return Task.CompletedTask;

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

        var namespaceReplacer
            = new ReplaceNamespace(containingNamespace.ToDisplayString(), context.Project.DefaultNamespace!);

        foreach (var typeSymbol in typeSymbols)
        {
            if (context.Items.Any(i => TypeName(i.SourceType) == typeSymbol.Name)) continue;

            var typeSyntax = context.Decompiler.GetSyntax(typeSymbol);
            if (typeSyntax is null) continue;
            typeSyntax = namespaceReplacer.Visit(typeSyntax);

            foreach (var rewriter in Rewriters)
            {
                typeSyntax = rewriter switch
                {
                    ContextAwareCSharpSyntaxRewriter contextRewriter => contextRewriter.Visit(typeSyntax, context),
                    _ => rewriter.Visit(typeSyntax)
                };
            }

            var fileName = Path.Combine(OutputPath ?? context.Generator.OutputPath, $"{typeSymbol.Name}.cs");
            var document = context.Project.AddDocument(fileName, typeSyntax.NormalizeWhitespace().GetText());
            context.Project = document.Project;

            var type = ParseTypeName(typeSymbol.Name);
            var qualifiedSourceType = ParseTypeName($"{context.ApiTypeSymbol.ContainingNamespace.ToDisplayString()}.{typeSymbol.Name}");

            context.Items.Add(new GeneratorItem(typeSymbol.Name, type, type, qualifiedSourceType, document.Id));
        }

        return Task.CompletedTask;
    }

    public override string ToString()
    {
        return $"{GetType().Name} - {TypeKind}";
    }
}
