global using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using SilkWrapped.ObjectModelTool.SyntaxTransformers;

namespace SilkWrapped.ObjectModelTool;

internal class Program
{
    // https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtokensource
    private static readonly CancellationTokenSource cts = new();



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
    /// <param name="whatIf"></param>
    private static async Task Main(
        FileInfo projectPath,
        string containerType,
        string apiOwnerType,
        string outputPath = @".\Generated",
        string wrapperNameFormatString = GeneratorOptions.DefaultWrapperNameFormatString,
        string constructionMethodNamePattern = GeneratorOptions.DefaultConstructionMethodNamePattern,
        string disposalMethodNamePattern = GeneratorOptions.DefaultDisposalMethodNamePattern,
        string handleTypeNameExclusionPattern = GeneratorOptions.DefaultHandleTypeNameExclusionPattern,
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


        if (projectPath is not { Exists: false })
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
        string? projectBackup = null;
        if (!whatIf)
        {
            projectBackup = await File.ReadAllTextAsync(projectPath.FullName, token);
        }

        using var workspace = MSBuildWorkspace.Create();
        var project = await workspace.OpenProjectAsync(projectPath.FullName);
        var compilation = await project.GetCompilationAsync(token);

        if (compilation is null)
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
        objectGenerator.CollectTypeInformation(token);

        var decompilerOptions = DecompilerOptions.Default with
        {
            Settings = new()
            {

            },

            Filter = (ts) =>
            {
                switch (ts.TypeKind)
                {
                    case TypeKind.Enum:
                        break;
                    case TypeKind.Struct:
                        if (ts.Name.StartsWith("Pfn")) return false;
                        if (ts.Name.StartsWith("ShaderModule")) return false;
                        if (ts.Name.StartsWith("ChainedStruct")) return false;
                        break;
                    default:
                        return false;
                }

                return !objectGenerator.IsHandleType(ts, true);
            },

            Rewriters =
            [
                new ReplaceNamespace(apiOwnerTypeSymbol.ContainingNamespace.ToDisplayString(), containerTypeSymbol.ContainingNamespace.ToDisplayString()),
                new RemoveAttributes("NativeName"),
                new BytePointerToString("Label", "Key"),
                new RemoveChainingStruct(),
                new TypeReplacer(objectGenerator.GetHandleTypeMap(true)),
                new PointerToSpan(),
                new MakeStructPartial(),
                new PointerToNullableType()
            ]
        };

        var decompiler = new Decompiler(compilation, apiOwnerTypeSymbol, decompilerOptions);

        foreach (var item in decompiler.GetTypes())
        {
            SyntaxNode syntax = item.DecompiledSyntax;

            string subDirectory = item.Symbol.TypeKind switch
            {
                TypeKind.Struct => nameof(TypeKind.Struct),
                TypeKind tk => tk.ToString(),
            };

            var fileName = Path.Combine(outputDrectory.FullName, subDirectory, $"{item.Symbol.Name}.cs");

            string directory = Path.Combine(outputDrectory.FullName, subDirectory);
            Directory.CreateDirectory(directory);
            var document = project.AddDocument(fileName, syntax.GetText());

            project = document.Project;


            if (whatIf)
            {
                Console.WriteLine($"Will create '{fileName}'");
            }
        }

        Console.WriteLine(new string('-', 80));

        foreach ((string Name, string Source) in objectGenerator.GetSources(token))
        {
            var fileName = Path.Combine(outputDrectory.FullName, $"{Name}.cs");
            var document = project.AddDocument(fileName, SourceText.From(Source));
            project = document.Project;
            if (whatIf)
            {
                Console.WriteLine($"Will create '{fileName}'");
            }
        }

        if (!whatIf)
        {
            workspace.TryApplyChanges(project.Solution);

            //HACK: Calling AddDocument() on a SDK-Style Project Adds a Compile Element
            // https://github.com/dotnet/roslyn/issues/36781
            await File.WriteAllTextAsync(projectPath.FullName, projectBackup, Encoding.UTF8, token);
        }
    }
}
