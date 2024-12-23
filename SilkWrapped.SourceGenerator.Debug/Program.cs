using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SilkWrapped.SourceGenerator.Common;

var source =
"""
using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace SilkWrapped.WebGPU.Example;

[VertexStruct]
[StructLayout(LayoutKind.Sequential)]
internal readonly partial record struct Vertex(Vector2 Position, Vector2 TexCoord);

""";

var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest));

var types = new[]
{
    typeof(Silk.NET.WebGPU.WebGPU).GetTypeInfo(),
    typeof(Silk.NET.WebGPU.Extensions.Dawn.Dawn).GetTypeInfo(),
    typeof(SilkWrapped.WebGPU.Device).GetTypeInfo(),
    typeof(SilkWrapped.WebGPU.GraphicsDeviceManager).GetTypeInfo(),
};

var metadataReferences = AppDomain.CurrentDomain.GetAssemblies().Select(a => MetadataReference.CreateFromFile(a.Location)).ToList();

var compilation = CSharpCompilation.Create("compilation",
                [syntaxTree],
                metadataReferences,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

foreach (var item in compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error))
{
    Console.WriteLine(item.GetMessage());
}


//GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

////driver = driver.RunGenerators(compilation!);
//driver = driver.RunGeneratorsAndUpdateCompilation(compilation!, out var outputCompilation, out var diagnostics);

//if (Directory.Exists("sourceout"))
//{
//    Directory.GetFiles("sourceout").ToList().ForEach(f => File.Delete(f));
//}
//else
//{
//    Directory.CreateDirectory("sourceout");
//}

//foreach (var item in outputCompilation.SyntaxTrees.Where( t => !string.IsNullOrEmpty(t.FilePath)))
//{
//    File.WriteAllText($@"sourceout\{Path.GetFileName(item.FilePath)}", item.GetText().ToString());
//}


Console.WriteLine();