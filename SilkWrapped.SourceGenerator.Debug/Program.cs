// See https://aka.ms/new-console-template for more information


using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.MSBuild;
using SilkWrapped.SourceGenerator;
using SilkWrapped.SourceGenerator.Debug;

var targetProjectPath = @"..\..\..\..\SilkWrapped.WebGPU\SilkWrapped.WebGPU.csproj";

MSBuildLocator.RegisterDefaults();

using var workspace = MSBuildWorkspace.Create();

var project = await workspace.OpenProjectAsync(targetProjectPath);

var compilation = await project.GetCompilationAsync();

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

Console.WriteLine();