using Microsoft.CodeAnalysis;

namespace SilkWrapped.ObjectModelTool;
internal class GeneratorTransformContext
{
    public GeneratorTransformContext(GeneratorConfig config, Project project, INamedTypeSymbol apiTypeSymbol, INamedTypeSymbol apiOwnerTypeSymbol)
    {
        this.Config = config;
        this.Project = project;
        this.ApiTypeSymbol = apiTypeSymbol;
        this.ApiOwnerTypeSymbol = apiOwnerTypeSymbol;

        if(project.TryGetCompilation(out var compilation))
        {
            Decompiler = new Decompiler(compilation, apiTypeSymbol);
        }
    }

    public Project Project { get; set; }
    public INamedTypeSymbol ApiTypeSymbol { get; set; }
    public INamedTypeSymbol ApiOwnerTypeSymbol { get; set; }
    internal GeneratorConfig Config { get; set; }
    internal Decompiler? Decompiler { get; }

}
