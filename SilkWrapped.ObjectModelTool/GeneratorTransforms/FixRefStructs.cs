

using Microsoft.CodeAnalysis.CSharp.Syntax;
using SilkWrapped.ObjectModelTool.Rewriters;

namespace SilkWrapped.ObjectModelTool.GeneratorTransforms;
internal class FixRefStructs : GeneratorTransformBase
{
    public override async Task TransformAsync(GeneratorTransformContext context, CancellationToken cancellationToken)
    {
        //CS8345 Field or auto-implemented property cannot be of type '' unless it is an instance member of a ref struct.
        //CS9244 The type '' may not be a ref struct or a type parameter allowing ref structs in order to use it as parameter 'T' in the generic type or method 'Nullable<T>'

        IEnumerable<Diagnostic> diagnostics = GetRelevantDiagnostics(context, cancellationToken);

        while (diagnostics.Any())
        {
            foreach (var diagnostic in diagnostics)
            {
                if(diagnostic.Location.SourceTree is not SyntaxTree sourceTree) continue;
                if (context.Project.GetDocumentId(sourceTree) is not DocumentId documentId) continue;
                var typeSyntax = await context.GetSyntaxRootAsync(documentId, cancellationToken);

                if (typeSyntax is null) continue;


                if (diagnostic.Id is "CS8345")
                {
                    typeSyntax = MakeRefStruct.Instance.Visit(typeSyntax);
                    await context.UpdateDocumentAsync(documentId, typeSyntax, cancellationToken);
                    continue;
                }
                if (diagnostic.Id is "CS9244")
                {
                    typeSyntax = new MakeNullableRef(diagnostic.Location).Visit(typeSyntax);
                    await context.UpdateDocumentAsync(documentId, typeSyntax, cancellationToken);
                    continue;
                }
            }

            diagnostics = GetRelevantDiagnostics(context, cancellationToken);
        }

        

        static IEnumerable<Diagnostic> GetRelevantDiagnostics(GeneratorTransformContext context, CancellationToken cancellationToken)
        {
            var diagnostics = context.Compilation?.GetDeclarationDiagnostics(cancellationToken) ?? Enumerable.Empty<Diagnostic>();

            diagnostics = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
            diagnostics = diagnostics.Where(d => d.Id is "CS8345" or "CS9244");
            return diagnostics;
        }
    }
}
