using System.Collections;
using System.Diagnostics.CodeAnalysis;
using SilkWrapped.SourceGenerator;

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