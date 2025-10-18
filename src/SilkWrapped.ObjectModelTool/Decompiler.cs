using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;

namespace SilkWrapped.ObjectModelTool;

internal record DecompilerOptions()
{
    public DecompilerSettings Settings { get; init; } = new DecompilerSettings();
}

internal class Decompiler
{
    private readonly IAssemblySymbol containingAssembly;
    private readonly CSharpDecompiler? decompiler;

    public Decompiler(Compilation compilation, INamedTypeSymbol apiTypeSymbol, DecompilerSettings? settings = null)
    {
        this.containingAssembly = apiTypeSymbol.ContainingAssembly;

        settings ??= new DecompilerSettings()
        {

        };

        if (compilation.GetMetadataReference(containingAssembly) is PortableExecutableReference { FilePath: string assemblyFileName })
        {
            decompiler = new CSharpDecompiler(assemblyFileName, new AssemblyResolver(compilation), settings);
        }
    }

    public SyntaxNode? GetSyntax(INamedTypeSymbol namedTypeSymbol)
    {
        var name = namedTypeSymbol.ToDisplayString();
        var source = decompiler!.DecompileTypeAsString(new ICSharpCode.Decompiler.TypeSystem.FullTypeName(name));
        return string.IsNullOrEmpty(source) ? null : SyntaxFactory.ParseSyntaxTree(source).GetRoot();
    }

    private class AssemblyResolver(Compilation compilation) : IAssemblyResolver
    {
        private readonly Dictionary<string, string> assemblyMap = compilation
                .GetUsedAssemblyReferences()
                .Where(r => r is not null)
                .ToDictionary(r => Path.GetFileNameWithoutExtension(r.Display!), r => r.Display!);

        public MetadataFile? Resolve(IAssemblyReference reference)
        {
            throw new NotImplementedException();
        }

        public Task<MetadataFile?> ResolveAsync(IAssemblyReference reference)
        {
            if (assemblyMap.TryGetValue(reference.Name, out var assembly))
            {
                return Task.FromResult<MetadataFile?>(new PEFile(assembly));
            }
            return Task.FromResult<MetadataFile?>(null);
        }

        public MetadataFile? ResolveModule(MetadataFile mainModule, string moduleName)
        {
            throw new NotImplementedException();
        }

        public Task<MetadataFile?> ResolveModuleAsync(MetadataFile mainModule, string moduleName)
        {
            throw new NotImplementedException();
        }
    }
}
