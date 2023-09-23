using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SilkWrapped.SourceGenerator;
using SilkWrapped.SourceGenerator.Debug;

var source =
"""

""";

var types = new[]
{
    typeof(Silk.NET.WebGPU.WebGPU).GetTypeInfo(),
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

var config = new TestAnalyzerConfigOptionsProvider()
{
    GlobalOptions =
    {
        ["RootNamespace"] = "SilkWrapped.WebGPU",
        ["SilkObjectModel_API"] = "Silk.NET.WebGPU.WebGPU",
        ["SilkObjectModel_APIOwnerTypeName"] = "Silk.NET.WebGPU.Instance",
        ["SilkObjectModel_Extensions"] = "Silk.NET.WebGPU.Extensions.Dawn.Dawn;Silk.NET.WebGPU.Extensions.WGPU.Wgpu",
    }
};

GeneratorDriver driver = CSharpGeneratorDriver.Create(generator).WithUpdatedAnalyzerConfigOptions(config);

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