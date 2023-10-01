using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SilkWrapped.SourceGenerator;
using SilkWrapped.SourceGenerator.Debug;

var source =
"""
using Silk.NET.WebGPU.Extensions.Dawn;
using SilkWrapped.SourceGenerator;

namespace SilkWrapped.WebGPU;

[ApiContainer(typeof(Silk.NET.WebGPU.Instance), HandleTypeNameExclusionPattern = "(Pfn).*|.*Descriptor|InstanceFeatures|Future")]
public unsafe partial class ApiContainer
{
    public ApiContainer()
    {
        Core = Silk.NET.WebGPU.WebGPU.GetApi();
        if(Core.TryGetDeviceExtension(null, out Dawn dawn))
        {
            Dawn = dawn;
        }
    }

    public Silk.NET.WebGPU.WebGPU Core { get; }
    public Dawn? Dawn { get; set; }
}
""";

var types = new[]
{
    typeof(Silk.NET.WebGPU.WebGPU).GetTypeInfo(),
    typeof(Silk.NET.WebGPU.Extensions.Dawn.Dawn).GetTypeInfo(),
};

var metadataReferences = AppDomain.CurrentDomain.GetAssemblies().Select(a => MetadataReference.CreateFromFile(a.Location)).ToList();
  
var compilation = CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest)) },
                metadataReferences,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

foreach (var item in compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error))
{
    Console.WriteLine(item.GetMessage());
}


var generator = new ObjectModelSourceGenerator();

GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

//driver = driver.RunGenerators(compilation!);
driver = driver.RunGeneratorsAndUpdateCompilation(compilation!, out var outputCompilation, out var diagnostics);

if (Directory.Exists("sourceout"))
{
    Directory.GetFiles("sourceout").ToList().ForEach(f => File.Delete(f));
}
else
{
    Directory.CreateDirectory("sourceout");
}

foreach (var item in outputCompilation.SyntaxTrees.Where( t => !string.IsNullOrEmpty(t.FilePath)))
{
    File.WriteAllText($@"sourceout\{Path.GetFileName(item.FilePath)}", item.GetText().ToString());
}


Console.WriteLine();