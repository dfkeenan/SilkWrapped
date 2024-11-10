using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
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

        //TODO: Validations
        if (projectPath is null or { Exists: false })
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
            Filter = (ts) =>
            {
                switch (ts.TypeKind)
                {
                    case TypeKind.Enum:
                    case TypeKind.Struct:
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
                new RemoveChainingPointers(),
                new TypeReplacer(objectGenerator.GetHandleTypeMap(true, true)),
                new MakeStructPartial()
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
            if (!whatIf)
            {
                Directory.CreateDirectory(Path.Combine(outputDrectory.FullName, subDirectory));

                File.WriteAllText(fileName, syntax.ToFullString());
            }
            else
            {
                Console.WriteLine($"Will create '{fileName}'");
            }
        }

        Console.WriteLine(new string('-', 80));

        foreach ((string Name, string Source) in objectGenerator.GetSources(token))
        {
            var fileName = Path.Combine(outputDrectory.FullName, $"{Name}.cs");
            if (!whatIf)
            {
                File.WriteAllText(fileName, Source);
            }
            else
            {
                Console.WriteLine($"Will create '{fileName}'");
            }
        }
    }
}
