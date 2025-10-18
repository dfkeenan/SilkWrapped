using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using SilkWrapped.ObjectModelTool.Rewriters;

namespace SilkWrapped.ObjectModelTool.GeneratorTransforms;
internal class RemoveUnsusedUsingDirectives : GeneratorTransformBase
{
    public override async Task TransformAsync(GeneratorTransformContext context, CancellationToken cancellationToken)
    {

        foreach (var document in context.Project.Documents)
        {
            var sematicModel = await document.GetSemanticModelAsync(cancellationToken);

            // Get diagnostics for the document
            var diagnostics = sematicModel?.GetDiagnostics() ?? Enumerable.Empty<Diagnostic>();

            // Filter for unused using diagnostics (e.g., CS8019)
            var unusedUsingDiagnostics = diagnostics.Where(d => d.Id == "CS8019");

            if (!unusedUsingDiagnostics.Any()) continue;

            var typeSyntax = await context.GetSyntaxRootAsync(document.Id, cancellationToken);

            if (typeSyntax is null) continue;

            // Get the corresponding UsingDirectiveSyntax nodes

            var unusedUsingNodes = unusedUsingDiagnostics
                .Select(d => (UsingDirectiveSyntax)typeSyntax.FindNode(d.Location.SourceSpan));

            typeSyntax = new UnusedUsingRemover(unusedUsingNodes).Visit(typeSyntax);

            await context.UpdateDocumentAsync(document.Id, typeSyntax, cancellationToken);
        }
       
    }

    public class UnusedUsingRemover : CSharpSyntaxRewriter
    {
        private readonly HashSet<UsingDirectiveSyntax> _unusedUsings;

        public UnusedUsingRemover(IEnumerable<UsingDirectiveSyntax> unusedUsings)
        {
            _unusedUsings = new HashSet<UsingDirectiveSyntax>(unusedUsings);
        }

        public override SyntaxNode VisitUsingDirective(UsingDirectiveSyntax node)
        {
            // If the current using directive is in our set of unused usings, remove it.
            // Returning null effectively removes the node from the syntax tree.
            if (_unusedUsings.Contains(node))
            {
                // You might want to handle trivia (comments, whitespace) associated with the removed node.
                // For example, you could attach leading trivia to the next node or discard it.
                // For simplicity, this example discards it.
                return null;
            }

            // Otherwise, visit the node's children as usual.
            return base.VisitUsingDirective(node);
        }
    }
}
