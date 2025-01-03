using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SilkWrapped.SourceGenerator.Common;

namespace SilkWrapped.WebGPU.Extensions.SourceGenerator;

[Generator]
public class BindGroupSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var declarations = context.SyntaxProvider.ForAttributeWithMetadataName(
            "SilkWrapped.WebGPU.BindGroupAttribute",
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
        if (node is not ClassDeclarationSyntax decl) return false;
        if (!decl.IsPartial()) return false;
        if (decl.ParameterList is null) return false;
        

        return true;
    }

    private static readonly SymbolDisplayFormat TypeDisplayFormat = SymbolDisplayFormat.FullyQualifiedFormat
                                .RemoveMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    private static DeclartionInfo? GetDeclartionInfo(
        SemanticModel semanticModel,
        ISymbol targetSymbol,
        SyntaxNode targetNode,
        ImmutableArray<AttributeData> attributes)
    {
        if (targetNode is not ClassDeclarationSyntax decl) return null;
        if (targetSymbol is not INamedTypeSymbol namedType) return null;
        if (attributes is not [AttributeData attribute]) return null;



        return new DeclartionInfo(
            namedType.Name,
            namedType.ContainingNamespace.ToDisplayString(),
            decl.GetDeclaration());
    }


    private record DeclartionInfo(
        string Name,
        string Namespace,
        string Declaration)
    {
        public string HintName => $"{Namespace}.{Name}.g.s";

        public string GetSource()
        {
            var sb = new IndentedStringBuilder();
            var deviceName = "device";
            var layoutName = "layout";
            var groupName = "group";

            sb.AppendLine($"namespace {Namespace}");
            using (sb.BeginBlock())
            {
                sb.AppendLine(Declaration)
                    .AppendIndent().AppendLine($": {SGNamespaces.SWWebGPU["IBindGroup"]}<{Name}>");
                using (sb.BeginBlock())
                {
                    sb.AppendCompilerGenerated();
                    sb.AppendLine($"private readonly {SGNamespaces.SWWebGPU["Device"]} {deviceName} = device;").AppendLine();
                    sb.AppendCompilerGenerated();
                    sb.AppendLine($"private {SGNamespaces.SWWebGPU["BindGroupLayout"]} {layoutName};").AppendLine();
                    sb.AppendCompilerGenerated();
                    sb.AppendLine($"private {SGNamespaces.SWWebGPU["BindGroup"]} {groupName};").AppendLine();

                    sb.AppendCompilerGenerated();
                    sb.AppendLine($"public {SGNamespaces.SWWebGPU["BindGroupLayout"]} Layout");
                    using (sb.BeginBlock())
                    {
                        sb.AppendLine("get");
                        using (sb.BeginBlock())
                        {
                            sb.AppendLine($"if ({layoutName} is not null) return {layoutName};");
                            //TODO: Workout how to cache/share layouts

                            sb.AppendLine($"{layoutName} = CreateLayout({deviceName});");
                            sb.AppendLine($"return {layoutName};");
                        }
                    }
                    sb.AppendLine();
                    sb.AppendCompilerGenerated();
                    sb.AppendLine($"public static {SGNamespaces.SWWebGPU["BindGroupLayout"]} CreateLayout({SGNamespaces.SWWebGPU["Device"]} device)");
                    using (sb.BeginBlock())
                    {
                        sb.AppendLine($"var descriptor = new {SGNamespaces.SWWebGPU["BindGroupLayoutDescriptor"]}");
                        using (sb.BeginBlock(separator: ';'))
                        {
                            sb.AppendLine("Entries = ");
                            using (sb.BeginBlock('['))
                            {
                                //TODO: BindGroupLayoutDescriptor.Entries
                            }
                        }

                        sb.AppendLine($"return {deviceName}.CreateBindGroupLayout(in descriptor);");
                    }
                    sb.AppendLine();

                    sb.AppendCompilerGenerated();
                    sb.AppendLine("private void CreateBindGroup()");
                    using (sb.BeginBlock())
                    {
                        sb.AppendLine($"if ({groupName} is not null) return;").AppendLine();

                        //TODO: Create buffers


                        sb.AppendLine($"var descriptor = new {SGNamespaces.SWWebGPU["BindGroupDescriptor"]}");
                        using (sb.BeginBlock(separator: ';'))
                        {
                            sb.AppendLine($"Layout = Layout,");
                            sb.AppendLine("Entries = ");
                            using (sb.BeginBlock('['))
                            {
                                //TODO: BindGroupDescriptor.Entries
                            }
                        }

                        sb.AppendLine($"{groupName} = {deviceName}.CreateBindGroup(in descriptor);");
                    }
                    sb.AppendLine();

                    sb.AppendCompilerGenerated();
                    sb.AppendLine("public void ApplyChanges()");
                    using (sb.BeginBlock())
                    {
                        sb.AppendLine($"CreateBindGroup();").AppendLine();

                        //TODO: Update buffers
                    }
                    sb.AppendLine();

                    sb.AppendCompilerGenerated();
                    sb.AppendLine("public void Dispose()");
                    using (sb.BeginBlock())
                    {
                        sb.AppendLine($"{groupName}?.Dispose();")
                          .AppendLine($"{groupName} = null;");
                        sb.AppendLine($"{layoutName}?.Dispose();")
                          .AppendLine($"{layoutName} = null;");
                        //TODO: Dispose buffers
                    }
                    sb.AppendLine();

                    sb.AppendCompilerGenerated();
                    sb.AppendLine($"public bool Equals({Name} other)");
                    using (sb.BeginBlock())
                    {
                        sb.AppendLine($"if (other is null) return false;")
                          .AppendLine("CreateBindGroup();")
                          .AppendLine("other.CreateBindGroup();")
                          .AppendLine("return group.Equals(other.group);");
                    }
                    sb.AppendLine();

                    sb.AppendCompilerGenerated();
                    sb.AppendLine("public override bool Equals(object obj)");
                    using (sb.BeginBlock())
                    {
                        sb.AppendLine($"if (obj is not {Name} other) return false;")
                          .AppendLine("return Equals(other);");
                    }
                    sb.AppendLine();

                    sb.AppendCompilerGenerated();
                    sb.AppendLine("public override int GetHashCode()");
                    using (sb.BeginBlock())
                    {
                        sb.AppendLine($"CreateBindGroup();")
                          .AppendLine($"return {groupName}.GetHashCode();");
                    }
                    sb.AppendLine();

                    sb.AppendCompilerGenerated()
                      .AppendLine($"public static implicit operator {SGNamespaces.SWWebGPU["BindGroupLayoutHandle"]}({Name} obj)")
                      .AppendIndent().AppendLine($"=> obj.Layout;");
                    sb.AppendLine();

                    sb.AppendCompilerGenerated();
                    sb.AppendLine($"public static explicit operator {SGNamespaces.SWWebGPU["BindGroupHandle"]}({Name} obj)");
                    using (sb.BeginBlock())
                    {
                        sb.AppendLine($"obj.CreateBindGroup();")
                          .AppendLine($"return obj.{groupName};");
                    }
                }

            }
            return sb.ToString();
        }
    }
}
