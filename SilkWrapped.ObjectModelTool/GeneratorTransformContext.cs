using System.Collections;
using System.Diagnostics.CodeAnalysis;
using SilkWrapped.SourceGenerator.Common;

namespace SilkWrapped.ObjectModelTool;
internal class GeneratorTransformContext
{
    public GeneratorTransformContext(Generator config, Project project, INamedTypeSymbol apiTypeSymbol, INamedTypeSymbol apiOwnerTypeSymbol)
    {
        this.Generator = config;
        this.Project = project;
        this.ApiTypeSymbol = apiTypeSymbol;
        this.ApiOwnerTypeSymbol = apiOwnerTypeSymbol;

        if (project.TryGetCompilation(out var compilation))
        {
            Decompiler = new Decompiler(compilation, apiTypeSymbol);
        }
    }

    public Project Project { get; set; }
    public INamedTypeSymbol ApiTypeSymbol { get; set; }
    public INamedTypeSymbol ApiOwnerTypeSymbol { get; set; }
    public Generator Generator { get; set; }
    public Decompiler? Decompiler { get; }
    public GeneratorItemCollection Items { get; } = [];

    public string ApiName => ApiTypeSymbol.Name;

    public Compilation? Compilation { get; internal set; }

    private Dictionary<string, INamedTypeSymbol> apiTypeSymbols = [];
    private Dictionary<string, INamedTypeSymbol> generatedTypeSymbols = [];
    private Dictionary<string, string> handleTypes = [];
    private Dictionary<string, string> objectTypes = [];
    private readonly HashSet<string> shouldSkipMarshalling = [];

    public async Task<GeneratorItem> AddItem(
        string? outputPath,
        string typeName,
        SyntaxNode typeSyntax,
        TypeSyntax type,
        TypeSyntax sourceType,
        CancellationToken cancellationToken,
        bool isObjectModel = false)
    {
        var fileName = Path.Combine(outputPath ?? Generator.OutputPath, $"{typeName}.cs");
        var document = Project.AddDocument(fileName, typeSyntax.NormalizeWhitespace().GetText());

        Project = document.Project;

        var qualifiedSourceType = ParseTypeName($"{ApiTypeSymbol.ContainingNamespace.ToDisplayString()}.{sourceType}");

        GeneratorItem item = new GeneratorItem(typeName, type, sourceType, qualifiedSourceType, document.Id, isObjectModel);
        Items.Add(item);

        Compilation = await Project.GetCompilationAsync();

        apiTypeSymbols.Clear();
        generatedTypeSymbols.Clear();

        return item;

    }

    public async Task<SyntaxNode?> GetSyntaxRootAsync(DocumentId documentId, CancellationToken cancellationToken)
    {
        var document = Project.GetDocument(documentId)!;
        var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken);

        if (syntaxTree is null) return null;

        return syntaxTree.GetRoot();
    }

    public async Task UpdateDocumentAsync(DocumentId documentId, SyntaxNode typeSyntax, CancellationToken cancellationToken)
    {
        var document = Project.GetDocument(documentId)!;
        document = document.WithSyntaxRoot(typeSyntax.NormalizeWhitespace());
        Project = document.Project;
        Compilation = await Project.GetCompilationAsync();

        apiTypeSymbols.Clear();
        generatedTypeSymbols.Clear();
    }

    public bool TryGetApiTypeSymbol(string name, [NotNullWhen(true)] out INamedTypeSymbol? namedTypeSymbol)
    {
        if (apiTypeSymbols.Count == 0)
        {
            var apiNamespace = Compilation?.GetTypesByMetadataName(ApiTypeSymbol.ToDisplayString())
                                      .FirstOrDefault()?.ContainingNamespace;

            foreach (var symbol in apiNamespace?.GetTypeMembers() ?? [])
            {
                apiTypeSymbols[symbol.Name] = symbol;
            }
        }

        return apiTypeSymbols.TryGetValue(name, out namedTypeSymbol);
    }

    public bool TryGetGeneratedTypeSymbol(string name, [NotNullWhen(true)] out INamedTypeSymbol? namedTypeSymbol)
    {
        if (generatedTypeSymbols.Count == 0)
        {
            var generatedNamespaceParts = Project.DefaultNamespace?.Split('.') ?? [];
            var generatedNamespace = Compilation!.GlobalNamespace;

            foreach (var part in generatedNamespaceParts)
            {
                generatedNamespace = generatedNamespace.GetNamespaceMembers().First(m => m.Name == part);
                if (generatedNamespace is null) break;
            }

            foreach (var symbol in generatedNamespace?.GetTypeMembers() ?? [])
            {
                generatedTypeSymbols[symbol.Name] = symbol;
            }
        }

        return generatedTypeSymbols.TryGetValue(name, out namedTypeSymbol);
    }

    public bool IsGeneratedTypeSymbol(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol) return false;

        if (namedTypeSymbol.IsNullableOfT(out var outType))
        {
            namedTypeSymbol = outType!;
        }

        if (TryGetGeneratedTypeSymbol(namedTypeSymbol.Name, out var generatedTypeSymbol) &&
            SymbolEqualityComparer.Default.Equals(namedTypeSymbol.ContainingNamespace, generatedTypeSymbol.ContainingNamespace)) return true;

        return false;
    }

    public bool IsBlittable(IArrayTypeSymbol typeSymbol)
    {
        if (typeSymbol.ElementType is INamedTypeSymbol namedTypeSymbol)
        {
            return IsBlittable(namedTypeSymbol);
        }

        return false;
    }

    public bool IsBlittable(INamedTypeSymbol namedTypeSymbol)
    {
        if (namedTypeSymbol.TypeKind == TypeKind.Enum) return true;
        if (namedTypeSymbol.SpecialType == SpecialType.System_String) return false;

        if (namedTypeSymbol.TypeKind == TypeKind.Struct)
        {
            if (IsHandleType(namedTypeSymbol, out _)) return true;

            if (TryGetGeneratedTypeSymbol(namedTypeSymbol.Name, out var generatedType) &&
               TryGetApiTypeSymbol(namedTypeSymbol.Name, out var apiType))
            {
                var apiMembers = apiType.GetMembers().OfType<IFieldSymbol>().ToArray();
                var generatedMembers = generatedType.GetMembers().OfType<IFieldSymbol>().ToArray();

                if (apiMembers.Length != generatedMembers.Length) return false;

                foreach (var member in generatedMembers)
                {
                    if (member.Type is IArrayTypeSymbol) return false;
                    if (member.Type is not INamedTypeSymbol memberType) return false;
                    if (!IsBlittable(memberType)) return false;
                }
                return true;
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    public bool IsHandleType(INamedTypeSymbol namedTypeSymbol, [NotNullWhen(true)] out INamedTypeSymbol? apiTypeSymbol)
    {
        if (handleTypes.ContainsKey(namedTypeSymbol.Name))
        {
            var apiTypeName = namedTypeSymbol.Name[..^"Handle".Length];
            if (TryGetApiTypeSymbol(apiTypeName, out apiTypeSymbol)) return true;
        }
        apiTypeSymbol = null;
        return false;
    }

    public void SkipMarhsalling(string name)
        => shouldSkipMarshalling.Add(name);

    public bool ShouldMarshall(string name)
        => !shouldSkipMarshalling.Contains(name);

    public void AddHandle(string handleTypeName, string objectTypeName)
    {
        handleTypes.Add(handleTypeName, objectTypeName);
        objectTypes.Add(objectTypeName, handleTypeName);
    }

    public bool TryGetObjectTypeName(string handleTypeName, [NotNullWhen(true)] out string? objectTypeName)
    {
        return handleTypes.TryGetValue(handleTypeName, out objectTypeName);
    }

    public bool TryGetHandleTypeName(string objectTypeName, [NotNullWhen(true)] out string? handleTypeName)
    {
        return objectTypes.TryGetValue(objectTypeName, out handleTypeName);
    }
}

internal record GeneratorItem(
    string TypeName,
    TypeSyntax TypeSyntax,
    TypeSyntax SourceType,
    TypeSyntax QualifiedSourceType,
    DocumentId DocumentId,
    bool IsObjectModel = false)
{
    [MemberNotNullWhen(true, nameof(IsObjectModel))]
    public TypeSyntax? HandleType
        => IsObjectModel
                ? ParseTypeName($"{CustomSyntaxFactory.TypeName(SourceType)}Handle")
                : null;
};

internal class GeneratorItemCollection : ICollection<GeneratorItem>
{
    private readonly List<GeneratorItem> items = [];

    public int Count => items.Count;

    public bool IsReadOnly => false;

    public void Add(GeneratorItem item)
    {
        items.Add(item);
    }

    public void Clear()
    {
        items.Clear();
    }

    public bool Contains(GeneratorItem item)
    {
        return items.Contains(item);
    }

    public void CopyTo(GeneratorItem[] array, int arrayIndex)
    {
        items.CopyTo(array, arrayIndex);
    }

    public IEnumerator<GeneratorItem> GetEnumerator()
    {
        return ((IEnumerable<GeneratorItem>)items).GetEnumerator();
    }

    public bool Remove(GeneratorItem item)
    {
        return items.Remove(item);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)items).GetEnumerator();
    }
}