using System.Collections.Immutable;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SilkWrapped.ObjectModelTool;

internal record DecompiledTypeInfo(INamedTypeSymbol Symbol, SyntaxNode DecompiledSyntax);

internal record DecompilerOptions()
{
    public DecompilerSettings Settings { get; init; } = new DecompilerSettings();

    public Func<ITypeSymbol, bool> Filter { get; init; } = (_ => true);

    public ImmutableList<CSharpSyntaxRewriter> Rewriters { get; init; } = [];

    public static DecompilerOptions Default { get; } = new DecompilerOptions();
}

internal class Decompiler
{
    private readonly IAssemblySymbol containingAssembly;
    private readonly ImmutableArray<INamedTypeSymbol> namedTypeSymbols;
    private readonly DecompilerOptions options;
    private CSharpDecompiler? decompiler;

    public Decompiler(Compilation compilation, INamedTypeSymbol apiOwnerTypeSymbol, DecompilerOptions? options = null)
    {
        this.containingAssembly = apiOwnerTypeSymbol.ContainingAssembly;
        this.namedTypeSymbols = apiOwnerTypeSymbol.ContainingNamespace.GetTypeMembers();
        this.options = options ?? DecompilerOptions.Default;

        if (compilation.GetMetadataReference(containingAssembly) is PortableExecutableReference { FilePath: string assemblyFileName } reference)
        {
            DecompilerSettings settings = new DecompilerSettings();

            decompiler = new CSharpDecompiler(assemblyFileName, new AssemblyResolver(compilation), settings);

        }

    }

    public IEnumerable<DecompiledTypeInfo> GetTypes()
    {
        if(decompiler is null)
            yield break;

        foreach (var namedTypeSymbol in namedTypeSymbols)
        {
            if (!options.Filter(namedTypeSymbol)) continue;

            SyntaxNode syntax = GetSyntax(namedTypeSymbol);

            foreach (var rewriter in options.Rewriters)
            {
                syntax = rewriter.Visit(syntax);
            }

            yield return new DecompiledTypeInfo(namedTypeSymbol, syntax);
        }
    }

    private SyntaxNode GetSyntax(INamedTypeSymbol namedTypeSymbol)
    {
        var name = namedTypeSymbol.ToDisplayString();
        var source = decompiler!.DecompileTypeAsString(new ICSharpCode.Decompiler.TypeSystem.FullTypeName(name));
        return SyntaxFactory.ParseSyntaxTree(source).GetRoot();
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
            if(assemblyMap.TryGetValue(reference.Name, out var assembly))
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
