using Microsoft.CodeAnalysis.CSharp;
using SilkWrapped.ObjectModelTool.Rewriters;

namespace SilkWrapped.ObjectModelTool.GeneratorTransforms;

internal class ImplementObjectModelTypes : GeneratorTransformBase
{

    public List<CSharpSyntaxRewriter> Rewriters { get; set; } = [];

    public override async Task TransformAsync(GeneratorTransformContext context, CancellationToken cancellationToken)
    {
        foreach (var item in context.Items.Where(i => i.IsObjectModel))
        {
            var document = context.Project.GetDocument(item.DocumentId)!;
            var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken);

            if (syntaxTree is null) continue;

            var typeSyntax = syntaxTree.GetRoot();

            foreach (var rewriter in Rewriters)
            {
                typeSyntax = rewriter switch
                {
                    ContextAwareCSharpSyntaxRewriter contextRewriter => contextRewriter.Visit(typeSyntax, context),
                    _ => rewriter.Visit(typeSyntax)
                };

                document = document.WithSyntaxRoot(typeSyntax);
                context.Project = document.Project;
                context.Compilation = await context.Project.GetCompilationAsync();
            }

           
        }
    }
}