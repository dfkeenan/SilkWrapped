using SilkWrapped.ObjectModelTool.Rewriters;

namespace SilkWrapped.ObjectModelTool.GeneratorTransforms;

internal class ImplementObjectModelTypes : GeneratorTransformBase
{

    public List<CSharpSyntaxRewriter> Rewriters { get; set; } = [];

    public override async Task TransformAsync(GeneratorTransformContext context, CancellationToken cancellationToken)
    {
        foreach (var rewriter in Rewriters)
        {
            foreach (var item in context.Items.Where(i => i.IsObjectModel))
            {
                var typeSyntax = await context.GetSyntaxRootAsync(item.DocumentId, cancellationToken);

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
}