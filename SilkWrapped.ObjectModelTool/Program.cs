using Microsoft.CodeAnalysis.MSBuild;

namespace SilkWrapped.ObjectModelTool;

internal class Program
{
    /// <summary>
    /// Generates safe object model types for Silk.NET APIs
    /// </summary>
    /// <param name="projectPath"></param>
    /// <param name="outputPath"></param>
    /// <param name="containerType"></param>
    /// <param name="apiOwnerType"></param>
    /// <param name="wrapperNameFormatString"></param>
    /// <param name="constructionMethodNamePattern"></param>
    /// <param name="disposalMethodNamePattern"></param>
    /// <param name="handleTypeNameExclusionPattern"></param>
    static async Task Main(
        FileInfo projectPath,
        string containerType,
        string apiOwnerType,
        string outputPath = @".\Generated",
        string wrapperNameFormatString = GeneratorOptions.DefaultWrapperNameFormatString,
        string constructionMethodNamePattern = GeneratorOptions.DefaultConstructionMethodNamePattern,
        string disposalMethodNamePattern = GeneratorOptions.DefaultDisposalMethodNamePattern,
        string handleTypeNameExclusionPattern = GeneratorOptions.DefaultHandleTypeNameExclusionPattern
        )
    {
        //TODO: Validations
        if (projectPath is null or { Exists: false})
        {
            Console.WriteLine("Valid project not supplied");
            return;
        }

        var outputDrectory = new DirectoryInfo(Path.Combine(projectPath!.Directory!.FullName, outputPath));

        if (outputDrectory.Exists)
        {
            var files = outputDrectory.GetFiles("*.cs", new EnumerationOptions { RecurseSubdirectories = true });
            foreach (var file in files)
            {
                file.Delete();
            }
        }
        else
        {
            outputDrectory.Create();
        }

            using var workspace = MSBuildWorkspace.Create();
        var project = await workspace.OpenProjectAsync(projectPath.FullName);
        var compilation = await project.GetCompilationAsync();

        if(compilation is null)
        {
            Console.WriteLine("Failed to get project compilation");
            return;
        }

        var containerTypeSymbol = compilation.Assembly.GetTypeByMetadataName(containerType);

        if (containerTypeSymbol is null)
        {
            Console.WriteLine($"Failed to get API container type '{containerType}'");
            return;
        }

        var apiOwnerTypeSymbol = compilation.GetTypeByMetadataName(apiOwnerType);

        if (apiOwnerTypeSymbol is null)
        {
            Console.WriteLine($"Failed to get API owner type '{apiOwnerType}'");
            return;
        }

        var options = new GeneratorOptions
        {
            WrapperNameFormatString = wrapperNameFormatString,
            ConstructionMethodNamePattern = constructionMethodNamePattern,
            DisposalMethodNamePattern = disposalMethodNamePattern,
            HandleTypeNameExclusionPattern = handleTypeNameExclusionPattern,
        };

        var objectGenerator = new ObjectModelGenerator(containerTypeSymbol, apiOwnerTypeSymbol, options);

        

        foreach((string Name, string Source) in objectGenerator.GetSources(CancellationToken.None))
        {
            var fileName = Path.Combine(outputDrectory.FullName, $"{Name}.cs");
            File.WriteAllText(fileName, Source);
        }
    }
}
