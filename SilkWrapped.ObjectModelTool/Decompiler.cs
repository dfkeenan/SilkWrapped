using System.Collections.Immutable;
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
    private readonly ImmutableArray<INamedTypeSymbol> namedTypeSymbols;
    private readonly DecompilerOptions options;
    private CSharpDecompiler? decompiler;

    public Decompiler(Compilation compilation, INamedTypeSymbol apiTypeSymbol, DecompilerSettings? settings = null)
    {
        this.containingAssembly = apiTypeSymbol.ContainingAssembly;
        this.namedTypeSymbols = apiTypeSymbol.ContainingNamespace.GetTypeMembers();

        settings ??= new DecompilerSettings()
        {

        };

        if (compilation.GetMetadataReference(containingAssembly) is PortableExecutableReference { FilePath: string assemblyFileName } reference)
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

    private class AssemblyResolver : IAssemblyResolver
    {
        private readonly Compilation compilation;
        private readonly Dictionary<string, string> assemblyMap;

        public AssemblyResolver(Compilation compilation)
        {
            this.compilation = compilation;
            assemblyMap = compilation
                .GetUsedAssemblyReferences()
                .Where(r => r is not null)
                .ToDictionary(r => Path.GetFileNameWithoutExtension(r.Display!), r => r.Display!);
        }

        public PEFile? Resolve(IAssemblyReference reference)
        {
            throw new NotImplementedException();
        }

        public Task<PEFile?> ResolveAsync(IAssemblyReference reference)
        {
            if (assemblyMap.TryGetValue(reference.Name, out var assembly))
            {
                return Task.FromResult<PEFile?>(new PEFile(assembly));
            }
            return Task.FromResult<PEFile?>(null);
        }

        public PEFile? ResolveModule(PEFile mainModule, string moduleName)
        {
            throw new NotImplementedException();
        }

        public Task<PEFile?> ResolveModuleAsync(PEFile mainModule, string moduleName)
        {
            throw new NotImplementedException();
        }
    }
}
