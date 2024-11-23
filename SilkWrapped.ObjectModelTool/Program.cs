global using Microsoft.CodeAnalysis;
global using Microsoft.CodeAnalysis.CSharp.Syntax;
global using Microsoft.CodeAnalysis.MSBuild;
global using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
global using static SilkWrapped.SourceGenerator.CustomSyntaxFactory;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace SilkWrapped.ObjectModelTool;

internal class Program
{
    // https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtokensource
    private static readonly CancellationTokenSource cts = new();



    /// <summary>
    /// Generates safe object model types for Silk.NET APIs
    /// </summary>
    /// <param name="generatorConfig">Generator config file path</param>
    /// <param name="whatIf"></param>
    private static async Task Main(
        FileInfo generatorConfig,
        bool whatIf = false
        )
    {
        //whatIf = true;


        Console.WriteLine("Application has started. Ctrl-C to end");

        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            Console.WriteLine("Cancel event triggered");
            cts.Cancel();
            eventArgs.Cancel = true;
        };

        var token = cts.Token;

        try
        {
            if (generatorConfig is not { Exists: true })
            {
                Console.WriteLine("Invalid generator config file path");
                return;
            }
            var configJson = await File.ReadAllTextAsync(generatorConfig.FullName);
            var options = new JsonSerializerOptions(JsonSerializerOptions.Default)
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                TypeInfoResolver = new PolymorphicTypeResolver(),
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };

            var generator = JsonSerializer.Deserialize<Generator>(configJson, options)!;

            var projecPath = Path.GetDirectoryName(generatorConfig.FullName)!;
            projecPath = Path.Combine(projecPath, generator.ProjectFilePath ?? "");
            var projectDirectoryPath = Path.GetDirectoryName(projecPath);


            if (!File.Exists(projecPath))
            {
                Console.WriteLine("Project '{projectPath}' does not exist.");
                return;
            }

            var outputDrectory = new DirectoryInfo(Path.Combine(projectDirectoryPath!, generator.OutputPath ?? ""));

            if (outputDrectory.Exists)
            {
                var files = outputDrectory.GetFiles("*.cs", new EnumerationOptions { RecurseSubdirectories = true });
                foreach (var file in files)
                {
                    if (!whatIf)
                    {
                        file.Delete();
                    }
                    else
                    {
                        Console.WriteLine($"Will delete '{file.FullName}'");
                    }
                }
            }
            else
            {
                if (!whatIf)
                {
                    outputDrectory.Create();
                }
                else
                {
                    Console.WriteLine($"Will create directory '{outputDrectory.FullName}'");
                }
            }

            Console.WriteLine(new string('-', 80));

            //HACK: Calling AddDocument() on a SDK-Style Project Adds a Compile Element
            // https://github.com/dotnet/roslyn/issues/36781
            byte[]? projectBackup = null;
            if (!whatIf)
            {
                projectBackup = await File.ReadAllBytesAsync(projecPath, token);
            }


            using var workspace = MSBuildWorkspace.Create();
            var project = await workspace.OpenProjectAsync(projecPath);
            var compilation = await project.GetCompilationAsync(token);

            if (compilation is null)
            {
                Console.WriteLine("Failed to get project compilation");
                return;
            }

            if (generator.ApiTypeName is null or [] || compilation.GetTypeByMetadataName(generator.ApiTypeName) is not INamedTypeSymbol apiTypeSymbol)
            {
                Console.WriteLine($"Failed to get API type '{generator.ApiTypeName}'");
                return;
            }

            if (generator.ApiOwnerTypeName is null or [] || compilation.GetTypeByMetadataName(generator.ApiOwnerTypeName) is not INamedTypeSymbol apiOwnerTypeSymbol)
            {
                Console.WriteLine($"Failed to get API owner type '{generator.ApiTypeName}'");
                return;
            }

            var generatorContext = new GeneratorTransformContext(generator, project, apiTypeSymbol, apiOwnerTypeSymbol);


            foreach (var transformer in generator.TransformGenerators)
            {
                Console.WriteLine($"Executing '{transformer.ToString()}'");
                await transformer!.TransformAsync(generatorContext, token);
            }

            if (!whatIf)
            {
                workspace.TryApplyChanges(generatorContext.Project.Solution);

                //HACK: Calling AddDocument() on a SDK-Style Project Adds a Compile Element
                // https://github.com/dotnet/roslyn/issues/36781
                await File.WriteAllBytesAsync(projecPath, projectBackup!, token);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Something went wrong!");
            Console.WriteLine(ex.Message);
        }
    }
}
