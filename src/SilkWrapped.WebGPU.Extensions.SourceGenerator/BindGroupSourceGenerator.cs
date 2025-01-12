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
        ImmutableArray<AttributeData> targetAttributes)
    {
        if (targetNode is not ClassDeclarationSyntax decl) return null;
        if (targetSymbol is not INamedTypeSymbol namedType) return null;
        if (decl.ParameterList?.Parameters is not SeparatedSyntaxList<ParameterSyntax> { Count: > 0 } parameters) return null;
        if (targetAttributes is not [AttributeData bindGroupAttribute]) return null;
        if (bindGroupAttribute.ConstructorArguments is not [{ Value: uint bindGroupIndex }]) return null;

        string? deviceParameterName = null;
        var bindings = ImmutableArray.CreateBuilder<BindingInfo>();

        foreach (var parameter in parameters)
        {
            if (semanticModel.GetDeclaredSymbol(parameter) is not IParameterSymbol parameterSymbol) continue;

            if (parameterSymbol.Type.GetFullyQualifiedMetadataName() == "SilkWrapped.WebGPU.Device")
            {
                deviceParameterName = parameterSymbol.Name;
                continue;
            }

            if (parameterSymbol.GetAttributes() is not { IsEmpty: false } attributes) continue;

            foreach (var attribute in attributes)
            {
                if (GetBindingInfo(parameterSymbol.Name, parameterSymbol.Type, attribute) is not BindingInfo bindingInfo) continue;
                bindings.Add(bindingInfo);
            }
        }

        if (deviceParameterName is null) return null;

        foreach (var member in decl.Members)
        {
            if (member is not PropertyDeclarationSyntax property) continue;
            if (!property.IsPartial()) continue;
            if (semanticModel.GetDeclaredSymbol(property) is not IPropertySymbol propertySymbol) continue;

            if (propertySymbol.GetAttributes() is not { IsEmpty: false } attributes) continue;

            foreach (var attribute in attributes)
            {
                if (GetBindingInfo(propertySymbol.Name, propertySymbol.Type, attribute) is not BindingInfo bindingInfo) continue;

                var isEquatable = propertySymbol.Type.ImplementsInterface("System.IEquatable`1");

                bindings.Add(bindingInfo with
                {
                    IsProperty = true,
                    Declaration = property.GetDeclaration(bindingInfo.Type),
                    IsEquatable = isEquatable
                });
            }

        }

        return new DeclartionInfo(
            namedType.Name,
            namedType.ContainingNamespace.ToDisplayString(),
            bindGroupIndex,
            decl.GetDeclaration(),
            deviceParameterName,
            bindings.ToImmutableArray());
    }

    private static BindingInfo? GetBindingInfo(string name, ITypeSymbol typeSymbol, AttributeData attribute)
    {
        if (attribute.AttributeClass is null) return null;

        switch (attribute.AttributeClass.GetFullyQualifiedMetadataName())
        {
            case "SilkWrapped.WebGPU.UniformBindingAttribute":
                {
                    if (attribute.ConstructorArguments is [{ Value: int visibility }])
                    {
                        bool hasDynamicOffset = false;
                        if (attribute.NamedArguments is [{ Key: "HasDynamicOffset", Value.Value: bool offset }])
                        {
                            hasDynamicOffset = offset;
                        }
                        return new BufferBindingInfo(
                            name,
                            typeSymbol.ToDisplayString(TypeDisplayFormat),
                            (ShaderStage)visibility,
                            BufferBindingType.Uniform, hasDynamicOffset);
                    }
                }
                break;
            case "SilkWrapped.WebGPU.SamplerBindingAttribute":
                {
                    if (attribute.ConstructorArguments is [{ Value: int type }, { Value: int visibility }])
                    {
                        return new SamplerBindingInfo(
                            name,
                            typeSymbol.ToDisplayString(TypeDisplayFormat),
                            (ShaderStage)visibility,
                            (SamplerBindingType)type);
                    }
                }
                break;
            case "SilkWrapped.WebGPU.TextureBindingAttribute":
                {
                    if (attribute.ConstructorArguments is [{ Value: int sampleType }, { Value: int viewDimension }, { Value: int visibility }])
                    {
                        bool multisampled = false;
                        if (attribute.NamedArguments is [{ Key: "Multisampled", Value.Value: bool multisampledValue }])
                        {
                            multisampled = multisampledValue;
                        }

                        return new TextureBindingInfo(
                            name,
                            typeSymbol.ToDisplayString(TypeDisplayFormat),
                            (ShaderStage)visibility,
                            (TextureSampleType)sampleType,
                            (TextureViewDimension)viewDimension,
                            multisampled);
                    }
                }
                break;
            //case "SilkWrapped.WebGPU.StorageBindingAttribute":
            //    break;
            default:
                return null;
        }
        return null;
    }

    private abstract record BindingInfo(string Name, string Type, ShaderStage Visibility)
    {
        public bool IsProperty { get; init; }
        public string? Declaration { get; init; }
        public bool IsEquatable { get; init; }
    }

    private record BufferBindingInfo(string Name, string Type, ShaderStage Visibility, BufferBindingType BufferBindingType, bool HasDynamicOffset)
        : BindingInfo(Name, Type, Visibility);

    private record SamplerBindingInfo(string Name, string Type, ShaderStage Visibility, SamplerBindingType SamplerBindingType)
        : BindingInfo(Name, Type, Visibility);

    private record TextureBindingInfo(string Name, string Type, ShaderStage Visibility, TextureSampleType SampleType, TextureViewDimension ViewDimension, bool Multisampled)
        : BindingInfo(Name, Type, Visibility);

    private record DeclartionInfo(
        string Name,
        string Namespace,
        uint bindGroupIndex,
        string Declaration,
        string DeviceParameterName,
        EquatableArray<BindingInfo> Bindings)
    {
        public string HintName => $"{Namespace}.{Name}.g.s";

        public string GetSource()
        {
            var sb = new IndentedStringBuilder();
            var deviceName = "__device";
            var layoutName = "__layout";
            var groupName = "__group";
            var hasBuffers = Bindings.Any(b => b is BufferBindingInfo);

            sb.AppendLine($"namespace {Namespace}");
            using (sb.BeginBlock())
            {
                sb.AppendLine(Declaration)
                    .AppendIndent().AppendLine($": {SGNamespaces.SWWebGPU["IBindGroup"]}<{Name}>");
                using (sb.BeginBlock())
                {
                    sb.AppendCompilerGenerated(nameof(BindGroupSourceGenerator), false);
                    sb.AppendLine($"private readonly {SGNamespaces.SWWebGPU["Device"]} {deviceName} = {DeviceParameterName};").AppendLine();
                    sb.AppendCompilerGenerated(nameof(BindGroupSourceGenerator), false);
                    sb.AppendLine($"private {SGNamespaces.SWWebGPU["BindGroupLayout"]} {layoutName};").AppendLine();
                    sb.AppendCompilerGenerated(nameof(BindGroupSourceGenerator), false);
                    sb.AppendLine($"private {SGNamespaces.SWWebGPU["BindGroup"]} {groupName};").AppendLine();

                    if (hasBuffers)
                    {
                        sb.AppendCompilerGenerated(nameof(BindGroupSourceGenerator), false);
                        sb.AppendNeverEditorBrowsable();
                        if (Bindings.Length > 32)
                        {
                            sb.AppendLine("private ulong __changes = 0;");
                        }
                        else
                        {
                            sb.AppendLine("private uint __changes = 0;");
                        }
                    }
                    sb.AppendLine();

                    for (int i = 0; i < Bindings.Length; i++)
                    {
                        var binding = Bindings[i];
                        if (binding is not BufferBindingInfo { BufferBindingType: BufferBindingType.Uniform } bindingInfo)
                        {
                            if (!binding.IsProperty)
                            {
                                sb.AppendCompilerGenerated(nameof(BindGroupSourceGenerator));
                                sb.AppendLine($"public {binding.Type} {CustomSyntaxFactory.PascalCase(binding.Name)} => {binding.Name};");
                                sb.AppendLine();
                            }
                            continue;
                        }

                        sb.AppendCompilerGenerated(nameof(BindGroupSourceGenerator), false);
                        sb.AppendNeverEditorBrowsable();
                        sb.AppendLine($"private {SGNamespaces.SWWebGPU["Buffer"]}<{bindingInfo.Type}> __{bindingInfo.Name}Buffer;");
                        sb.AppendLine();

                        sb.AppendCompilerGenerated(nameof(BindGroupSourceGenerator), false);
                        sb.AppendNeverEditorBrowsable();
                        sb.AppendLine($"private {bindingInfo.Type} __{bindingInfo.Name};");
                        sb.AppendLine();

                        if (binding.IsProperty && bindingInfo.Declaration is not null)
                        {
                            sb.AppendLine(bindingInfo.Declaration);

                            using (sb.BeginBlock())
                            {
                                sb.AppendLine($"get => __{bindingInfo.Name};");
                                sb.AppendLine("set");
                                using (sb.BeginBlock())
                                {
                                    if (bindingInfo.IsEquatable)
                                    {
                                        sb.AppendLine($"if (global::System.Collections.Generic.EqualityComparer<{bindingInfo.Type}>.Default.Equals(__{bindingInfo.Name}, value)) return;");
                                    }
                                    sb.AppendLine($"__{bindingInfo.Name} = value;");
                                    sb.AppendLine($"__changes |= {1u << i};");
                                }
                            }
                        }

                        sb.AppendLine();
                    }

                    sb.AppendCompilerGenerated(nameof(BindGroupSourceGenerator));
                    sb.AppendLine($"public static uint BindGroupIndex => {bindGroupIndex};");
                    sb.AppendLine();


                    sb.AppendCompilerGenerated(nameof(BindGroupSourceGenerator));
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
                    sb.AppendCompilerGenerated(nameof(BindGroupSourceGenerator));
                    sb.AppendLine($"public static {SGNamespaces.SWWebGPU["BindGroupLayout"]} CreateLayout({SGNamespaces.SWWebGPU["Device"]} device)");
                    using (sb.BeginBlock())
                    {
                        sb.AppendLine($"var descriptor = new {SGNamespaces.SWWebGPU["BindGroupLayoutDescriptor"]}");
                        using (sb.BeginBlock(separator: ';'))
                        {
                            sb.AppendLine("Entries = ");
                            using (sb.BeginBlock('['))
                            {
                                for (int i = 0; i < Bindings.Length; i++)
                                {
                                    sb.AppendLine($"new {SGNamespaces.SWWebGPU["BindGroupLayoutEntry"]}");
                                    using (sb.BeginBlock(separator: ','))
                                    {
                                        sb.AppendLine($"Binding = {i},");
                                        switch (Bindings[i])
                                        {
                                            case BufferBindingInfo bufferBindingInfo:
                                                sb.AppendLine("Buffer = new ()");
                                                using (sb.BeginBlock(separator: ','))
                                                {
                                                    sb.AppendLine($"Type = {SGNamespaces.SWWebGPU["BufferBindingType"]}.{bufferBindingInfo.BufferBindingType},");
                                                    sb.AppendLine($"MinBindingSize = (ulong){CommonMethods.SizeOf(bufferBindingInfo.Type)},");
                                                    sb.AppendLine($"HasDynamicOffset = {bufferBindingInfo.HasDynamicOffset.ToString().ToLower()},");

                                                }
                                                break;
                                            case SamplerBindingInfo samplerBindingInfo:
                                                sb.AppendLine("Sampler = new ()");
                                                using (sb.BeginBlock(separator: ','))
                                                {
                                                    sb.AppendLine($"Type = {SGNamespaces.SWWebGPU["SamplerBindingType"]}.{samplerBindingInfo.SamplerBindingType},");
                                                }
                                                break;
                                            case TextureBindingInfo textureBindingInfo:
                                                sb.AppendLine("Texture = new ()");
                                                using (sb.BeginBlock(separator: ','))
                                                {
                                                    sb.AppendLine($"Multisampled = {textureBindingInfo.Multisampled.ToString().ToLower()},");
                                                    sb.AppendLine($"SampleType = {SGNamespaces.SWWebGPU["TextureSampleType"]}.{textureBindingInfo.SampleType},");
                                                    sb.AppendLine($"ViewDimension = {SGNamespaces.SWWebGPU["TextureViewDimension"]}.{textureBindingInfo.ViewDimension},");
                                                }
                                                break;

                                        }
                                        sb.AppendLine($"Visibility = {SGNamespaces.SWWebGPU["ShaderStage"]}.{Bindings[i].Visibility}");
                                    }
                                }
                            }
                        }

                        sb.AppendLine($"return device.CreateBindGroupLayout(in descriptor);");
                    }
                    sb.AppendLine();

                    sb.AppendCompilerGenerated(nameof(BindGroupSourceGenerator));
                    sb.AppendLine("private void CreateBindGroup()");
                    using (sb.BeginBlock())
                    {
                        sb.AppendLine($"if ({groupName} is not null) return;").AppendLine();

                        foreach (var binding in Bindings)
                        {
                            if (binding is not BufferBindingInfo { BufferBindingType: BufferBindingType.Uniform } bindingInfo) continue;
                            sb.AppendLine($"__{binding.Name}Buffer ??= {deviceName}.CreateBuffer<{bindingInfo.Type}>({SGNamespaces.SWWebGPU["BufferUsage"]}.Uniform | {SGNamespaces.SWWebGPU["BufferUsage"]}.CopyDst);");

                        }
                        sb.AppendLine();

                        sb.AppendLine($"var descriptor = new {SGNamespaces.SWWebGPU["BindGroupDescriptor"]}");
                        using (sb.BeginBlock(separator: ';'))
                        {
                            sb.AppendLine($"Layout = Layout,");
                            sb.AppendLine("Entries = ");
                            using (sb.BeginBlock('['))
                            {
                                for (int i = 0; i < Bindings.Length; i++)
                                {
                                    sb.AppendLine($"new {SGNamespaces.SWWebGPU["BindGroupEntry"]}");
                                    using (sb.BeginBlock(separator: ','))
                                    {
                                        sb.AppendLine($"Binding = {i},");
                                        switch (Bindings[i])
                                        {
                                            case BufferBindingInfo bufferBindingInfo:
                                                sb.AppendLine($"Buffer = __{bufferBindingInfo.Name}Buffer,");
                                                sb.AppendLine($"Size = (ulong){CommonMethods.SizeOf(bufferBindingInfo.Type)},");
                                                break;
                                            case SamplerBindingInfo samplerBindingInfo:
                                                sb.AppendLine($"Sampler = {samplerBindingInfo.Name},");
                                                break;
                                            case TextureBindingInfo textureBindingInfo:
                                                sb.AppendLine($"TextureView = {textureBindingInfo.Name},");
                                                break;

                                        }
                                    }
                                }
                            }
                        }

                        sb.AppendLine($"{groupName} = {deviceName}.CreateBindGroup(in descriptor);");
                    }
                    sb.AppendLine();

                    sb.AppendCompilerGenerated(nameof(BindGroupSourceGenerator));
                    sb.AppendLine("public void ApplyChanges()");
                    using (sb.BeginBlock())
                    {
                        sb.AppendLine($"CreateBindGroup();").AppendLine();

                        if (hasBuffers)
                        {
                            sb.AppendLine($"using var queue = {deviceName}.GetQueue();");

                            for (int i = 0; i < Bindings.Length; i++)
                            {
                                var binding = Bindings[i];
                                if (binding is not BufferBindingInfo { BufferBindingType: BufferBindingType.Uniform } bufferBindingInfo) continue;

                                var changeFlag = 1u << i;

                                sb.AppendLine($"if ((__changes & {changeFlag}) == {changeFlag})");
                                using (sb.BeginBlock())
                                {
                                    sb.AppendLine($"queue.WriteBuffer(__{binding.Name}Buffer, __{binding.Name});");
                                }
                            }

                            sb.AppendLine().AppendLine($"__changes = 0;");
                        }
                    }
                    sb.AppendLine();

                    sb.AppendCompilerGenerated(nameof(BindGroupSourceGenerator));
                    sb.AppendLine("public void Dispose()");
                    using (sb.BeginBlock())
                    {
                        sb.AppendLine($"{groupName}?.Dispose();")
                          .AppendLine($"{groupName} = null;");
                        sb.AppendLine($"{layoutName}?.Dispose();")
                          .AppendLine($"{layoutName} = null;");

                        if (hasBuffers)
                        {
                            foreach (var binding in Bindings)
                            {
                                if (binding is not BufferBindingInfo { BufferBindingType: BufferBindingType.Uniform } bufferBindingInfo) continue;

                                sb.AppendLine($"__{binding.Name}Buffer?.Dispose();");
                                sb.AppendLine($"__{binding.Name}Buffer = null;");
                            }
                        }
                    }
                    sb.AppendLine();

                    sb.AppendCompilerGenerated(nameof(BindGroupSourceGenerator));
                    sb.AppendLine($"public bool Equals({Name} other)");
                    using (sb.BeginBlock())
                    {
                        sb.AppendLine($"if (other is null) return false;")
                          .AppendLine("CreateBindGroup();")
                          .AppendLine("other.CreateBindGroup();")
                          .AppendLine($"return {groupName}.Equals(other.{groupName});");
                    }
                    sb.AppendLine();

                    sb.AppendCompilerGenerated(nameof(BindGroupSourceGenerator));
                    sb.AppendLine("public override bool Equals(object obj)");
                    using (sb.BeginBlock())
                    {
                        sb.AppendLine($"if (obj is not {Name} other) return false;")
                          .AppendLine("return Equals(other);");
                    }
                    sb.AppendLine();

                    sb.AppendCompilerGenerated(nameof(BindGroupSourceGenerator));
                    sb.AppendLine("public override int GetHashCode()");
                    using (sb.BeginBlock())
                    {
                        sb.AppendLine($"CreateBindGroup();")
                          .AppendLine($"return {groupName}.GetHashCode();");
                    }
                    sb.AppendLine();

                    sb.AppendCompilerGenerated(nameof(BindGroupSourceGenerator))
                      .AppendLine($"public static implicit operator {SGNamespaces.SWWebGPU["BindGroupLayoutHandle"]}({Name} obj)")
                      .AppendIndent().AppendLine($"=> obj.Layout;");
                    sb.AppendLine();

                    sb.AppendCompilerGenerated(nameof(BindGroupSourceGenerator));
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
