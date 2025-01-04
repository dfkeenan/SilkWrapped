using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SilkWrapped.SourceGenerator.Common;

namespace SilkWrapped.WebGPU.Extensions.SourceGenerator;

[Generator]
public class VectorStructSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var declarations = context.SyntaxProvider.ForAttributeWithMetadataName(
            "SilkWrapped.WebGPU.VertexStructAttribute",
            predicate: IsCandidate,
            transform: static (ctx, ct)
                => GetDeclartionInfo(ctx.TargetSymbol, ctx.TargetNode, ctx.Attributes))
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

    private static readonly SymbolDisplayFormat TypeDisplayFormat = SymbolDisplayFormat.FullyQualifiedFormat
                                .RemoveMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    private static DeclartionInfo? GetDeclartionInfo(
        ISymbol targetSymbol,
        SyntaxNode targetNode,
        ImmutableArray<AttributeData> attributes)
    {
        if (targetNode is not TypeDeclarationSyntax decl) return null;
        if (targetSymbol is not INamedTypeSymbol namedType) return null;
        if (attributes is not [AttributeData attribute]) return null;

        var fieldTypes = ImmutableArray.CreateBuilder<string>();

        foreach (var member in namedType.GetMembers())
        {
            if (member is IFieldSymbol { Type: INamedTypeSymbol fieldType })
            {

                fieldTypes.Add(fieldType.ToDisplayString(TypeDisplayFormat));
            }
        }

        var vertexStepMode = attribute.ConstructorArguments switch
        {
        [{ Value: int stepMode }] => (VertexStepMode)stepMode,
            _ => VertexStepMode.Vertex
        };

        return new DeclartionInfo(
            namedType.Name,
            namedType.ContainingNamespace.ToDisplayString(),
            decl.GetDeclaration(),
            vertexStepMode,
            fieldTypes.ToImmutable());
    }


    private record DeclartionInfo(
        string Name,
        string Namespace,
        string Declaration,
        VertexStepMode VertexStepMode,
        EquatableArray<string> FieldTypes)
    {
        public string HintName => $"{Namespace}.{Name}.g.s";

        public string GetSource()
        {
            var sb = new IndentedStringBuilder();

            sb.AppendLine($"namespace {Namespace}");
            using (sb.BeginBlock())
            {
                sb.AppendLine(Declaration)
                    .AppendIndent().AppendLine($": {SGNamespaces.SWWebGPU["IVertexStruct"]}");

                using (sb.BeginBlock())
                {
                    //sb.AppendCompilerGenerated().AppendNeverEditorBrowsable();
                    sb.AppendCompilerGenerated();
                    sb.AppendLine($"public static {SGNamespaces.SWWebGPU["VertexBufferLayout"]} GetLayout()");
                    using (sb.BeginBlock())
                    {
                        sb.AppendLine("int offset = 0;");
                        sb.AppendLine($"var attributes = new {SGNamespaces.SWWebGPU["VertexAttribute"]}[{FieldTypes.Length}];");
                        sb.AppendLine();

                        for (int i = 0; i < FieldTypes.Length; i++)
                        {
                            var typeName = FieldTypes[i];

                            sb.AppendLine($"attributes[{i}] = new ()");
                            using (sb.BeginBlock(separator: ';'))
                            {
                                if (CommonVertexFormats.TryGetFormat(typeName, out var format))
                                {
                                    sb.AppendLine($"Format = {format},");
                                }
                                else
                                {
                                    sb.AppendLine($"//Format = I don't Know,");
                                }

                                sb.AppendLine($"Offset = (ulong)offset,");
                                sb.AppendLine($"ShaderLocation = {i}");
                            }

                            if (i < FieldTypes.Length - 1)
                            {

                                sb.AppendLine($"offset += {CommonNamespaces.CompilerServices["Unsafe"]}.SizeOf<{typeName}>();");
                            }

                            sb.AppendLine();
                        }

                        sb.AppendLine($"var vertexBufferLayout = new {SGNamespaces.SWWebGPU["VertexBufferLayout"]}");
                        using (sb.BeginBlock(separator: ';'))
                        {
                            sb.AppendLine("Attributes = attributes,");
                            sb.AppendLine($"StepMode = {SGNamespaces.SWWebGPU["VertexStepMode"]}.{VertexStepMode},");
                            sb.AppendLine($"ArrayStride = (ulong){CommonNamespaces.CompilerServices["Unsafe"]}.SizeOf<{Name}>()");
                        }
                        
                        sb.AppendLine("return vertexBufferLayout;");
                    }

                }
            }
            return sb.ToString();
        }
    }
}