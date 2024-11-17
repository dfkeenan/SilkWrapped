global using Microsoft.CodeAnalysis;
global using Microsoft.CodeAnalysis.CSharp.Syntax;
global using Microsoft.CodeAnalysis.Editing;
global using Microsoft.CodeAnalysis.MSBuild;
global using Microsoft.CodeAnalysis.Text;
global using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
global using static SilkWrapped.SourceGenerator.CustomSyntaxFactory;

using System.Text.Json;
using System.Reflection;

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
            var config = JsonSerializer.Deserialize<GeneratorConfig>(configJson)!;

            var projecPath = Path.GetDirectoryName(generatorConfig.FullName)!;
            projecPath = Path.Combine(projecPath, config.ProjectFilePath ?? "");
            var projectDirectoryPath = Path.GetDirectoryName(projecPath);


            if (!File.Exists(projecPath)) 
            {
                Console.WriteLine("Project '{projectPath}' does not exist.");
                return;
            }

            var outputDrectory = new DirectoryInfo(Path.Combine(projectDirectoryPath!, config.OutputPath ?? ""));

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

            if (config.ApiTypeName is null or [] || compilation.GetTypeByMetadataName(config.ApiTypeName) is not INamedTypeSymbol apiTypeSymbol)
            {
                Console.WriteLine($"Failed to get API type '{config.ApiTypeName}'");
                return;
            }

            if (config.ApiOwnerTypeName is null or [] || compilation.GetTypeByMetadataName(config.ApiOwnerTypeName) is not INamedTypeSymbol apiOwnerTypeSymbol)
            {
                Console.WriteLine($"Failed to get API owner type '{config.ApiTypeName}'");
                return;
            }

            var generatorContext = new GeneratorTransformContext(config, project, apiTypeSymbol, apiOwnerTypeSymbol);

            var assembly = Assembly.GetExecutingAssembly();
            var types = assembly.GetTypes()
                                .Where(t => t.IsAssignableTo(typeof(GeneratorTransformBase)) && !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) is not null)
                                .ToDictionary(t => t.Name);


            foreach (var transformerName in config.TransformGenerators)
            {
                if (types.TryGetValue(transformerName, out var generatorType))
                {
                    Console.WriteLine($"Executing '{transformerName}'");
                    var transformer = Activator.CreateInstance(generatorType) as GeneratorTransformBase; 
                    await transformer!.TransformAsync(generatorContext, token);
                }
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

        //var objectGenerator = new ObjectModelGenerator(containerTypeSymbol, apiOwnerTypeSymbol, options);
        //objectGenerator.CollectTypeInformation(token);

        //var decompilerOptions = DecompilerOptions.Default with
        //{
        //    Settings = new()
        //    {

        //    },

        //    Filter = (ts) =>
        //    {
        //        switch (ts.TypeKind)
        //        {
        //            case TypeKind.Enum:
        //                break;
        //            case TypeKind.Struct:
        //                if (ts.Name.StartsWith("Pfn")) return false;
        //                if (ts.Name.StartsWith("ShaderModule")) return false;
        //                if (ts.Name.StartsWith("ChainedStruct")) return false;
        //                break;
        //            default:
        //                return false;
        //        }

        //        return !objectGenerator.IsHandleType(ts, true);
        //    },

        //    Rewriters =
        //    [
        //        new ReplaceNamespace(apiOwnerTypeSymbol.ContainingNamespace.ToDisplayString(), containerTypeSymbol.ContainingNamespace.ToDisplayString()),
        //        new RemoveAttributes("NativeName"),
        //        new BytePointerToString("Label", "Key"),
        //        new RemoveChainingStruct(),
        //        new TypeReplacer(objectGenerator.GetHandleTypeMap(true)),
        //        new PointerToSpan(),
        //        new MakeStructPartial(),
        //        new PointerToNullableType()
        //    ]
        //};

        //var decompiler = new Decompiler(compilation, apiOwnerTypeSymbol, decompilerOptions);

        //foreach (var item in decompiler.GetTypes())
        //{
        //    SyntaxNode syntax = item.DecompiledSyntax;

        //    string subDirectory = item.Symbol.TypeKind switch
        //    {
        //        TypeKind.Struct => nameof(TypeKind.Struct),
        //        TypeKind tk => tk.ToString(),
        //    };

        //    var fileName = Path.Combine(outputDrectory.FullName, subDirectory, $"{item.Symbol.Name}.cs");

        //    string directory = Path.Combine(outputDrectory.FullName, subDirectory);
        //    Directory.CreateDirectory(directory);
        //    var document = project.AddDocument(fileName, syntax.GetText());

        //    project = document.Project;


        //    if (whatIf)
        //    {
        //        Console.WriteLine($"Will create '{fileName}'");
        //    }
        //}

        //Console.WriteLine(new string('-', 80));

        //foreach ((string Name, string Source) in objectGenerator.GetSources(token))
        //{
        //    var fileName = Path.Combine(outputDrectory.FullName, $"{Name}.cs");
        //    var document = project.AddDocument(fileName, SourceText.From(Source));
        //    project = document.Project;
        //    if (whatIf)
        //    {
        //        Console.WriteLine($"Will create '{fileName}'");
        //    }
        //}


        //compilation = await project.GetCompilationAsync(token);

        //var diagnostics = compilation!.GetDiagnostics(token)
        //                             .Where(d => d.Severity == DiagnosticSeverity.Error);

        //foreach (var diagnosticGroup in diagnostics.GroupBy(d => d.Id))
        //{
        //    switch (diagnosticGroup.Key) 
        //    {
        //        case "CS8345": //ref field in non-ref struct
        //            //TODO: Doing this may cause more errors. would need to find references and fix them.
        //            foreach (var diagnostic in diagnosticGroup)
        //            {
        //                var source = diagnostic.Location.SourceTree;
        //                var document = project.GetDocument(source);
        //                var editor = await DocumentEditor.CreateAsync(document);

        //                SyntaxNode? nodeAtLocation = source!.GetRoot()?.FindNode(diagnostic.Location.SourceSpan);
        //                var structDeclaration = nodeAtLocation!.Ancestors().OfType<StructDeclarationSyntax>().FirstOrDefault();
        //                var updatedStructDeclaration = MakeRefStruct.Instance.Visit(structDeclaration);

        //                editor.ReplaceNode(structDeclaration, updatedStructDeclaration);

        //                var updatedDocument = editor.GetChangedDocument();
        //                project = updatedDocument.Project;
        //            }
        //            break;
        //        case "CS9244": //ref struct in Nullable<T>

        //            break;

        //    }
        //}

        //if (!whatIf)
        //{
        //    workspace.TryApplyChanges(project.Solution);

        //    //HACK: Calling AddDocument() on a SDK-Style Project Adds a Compile Element
        //    // https://github.com/dotnet/roslyn/issues/36781
        //    await File.WriteAllBytesAsync(projectPath.FullName, projectBackup!, token);
        //}
    }
}
