using System.Collections.Immutable;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SilkWrapped.SourceGenerator.Common;

namespace SilkWrapped.WebGPU.Extensions.SourceGenerator;

[Generator]
public class VectorStructSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var declarations = context.SyntaxProvider.ForAttributeWithMetadataName(
            "SilkWrapped.WebGPU.Extensions.VertexStructAttribute",
            predicate: IsCandidate,
            transform: static (ctx, ct) 
                => GetDeclartionInfo(ctx.SemanticModel, ctx.TargetSymbol, ctx.TargetNode, ctx.Attributes))
            .Where(i => i is not null);

        context.RegisterSourceOutput(
            declarations,
            static (ctx, source) =>
            {
                if (source is not null)
                {
                    ctx.AddSource(source.HintName, source.GetSource());
                }
            });
    }

    private static bool IsCandidate(SyntaxNode node, CancellationToken token)
    {
        if (node is not TypeDeclarationSyntax decl) return false;
        if (!decl.IsPartial()) return false;

        if (node is StructDeclarationSyntax) return true;
        if (node is RecordDeclarationSyntax rec && rec.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword)) return true;

        return false;
    }

    private static DeclartionInfo? GetDeclartionInfo(
        SemanticModel semanticModel, 
        ISymbol targetSymbol, 
        SyntaxNode targetNode, 
        ImmutableArray<AttributeData> attributes)
    {
        if (targetNode is not TypeDeclarationSyntax decl) return null;
        if (targetSymbol is not INamedTypeSymbol namedType) return null;

        return new DeclartionInfo(
            namedType.Name,
            namedType.ContainingNamespace.ToDisplayString(),
            decl.GetDeclaration(),
            []);
    }


    private record DeclartionInfo(
        string Name,
        string Namespace,
        string Declaration,
        ImmutableArray<INamedTypeSymbol> FieldTypes)
    {
        public string HintName => $"{Namespace}.{Name}.g.s";

        public string GetSource()
        {
            var sb = new IndentedStringBuilder();

            sb.AppendLine($"namespace {Namespace}").BlockStart();
            sb.AppendLine(Declaration)
              .AppendIndent().AppendLine($": global::SilkWrapped.WebGPU.Extensions.IVertexStruct")
              .BlockStart();



            sb.BlockEnd();
            sb.BlockEnd();
            return sb.ToString();
        }
    }
}