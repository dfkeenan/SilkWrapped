using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SilkWrapped.SourceGenerator.Debug;
using SilkWrapped.WebGPU.Extensions.SourceGenerator;

var source =
"""
using System.Numerics;
using System.Runtime.InteropServices;
using SilkWrapped.WebGPU;
//using Silk.NET.Maths;

namespace SilkWrapped.WebGPU.Example;

[VertexStruct]
[StructLayout(LayoutKind.Sequential)]
internal readonly partial record struct Vertex(Vector2 Position, Vector2 TexCoord, int Test);

[BindGroup]
internal partial class ProjectionMatrixBindGroup(Device device)
{
    [UniformBinding(ShaderStage.Vertex)]
    public partial Matrix4x4 Projection { get; set; }
}

[BindGroup]
internal partial class TextureBindGroup(
     Device device,
     [TextureBinding(TextureSampleType.Float, TextureViewDimension.Dimension2D, ShaderStage.Fragment)] TextureView textureView,
     [SamplerBinding(SamplerBindingType.Filtering, ShaderStage.Fragment)] Sampler sampler);

""";

var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest));

var types = new[]
{
    typeof(Silk.NET.WebGPU.WebGPU).GetTypeInfo(),
    typeof(Silk.NET.WebGPU.Extensions.Dawn.Dawn).GetTypeInfo(),
    typeof(SilkWrapped.WebGPU.Device).GetTypeInfo(),
    typeof(SilkWrapped.WebGPU.GraphicsDeviceManager).GetTypeInfo(),
};

var sourceGeneratorAssembly = typeof(VectorStructSourceGenerator).GetTypeInfo().Assembly;

var metadataReferences = AppDomain.CurrentDomain.GetAssemblies()
                                  .Where(a => a != sourceGeneratorAssembly)
                                  .Select(a => MetadataReference.CreateFromFile(a.Location)).ToList();

var compilation = CSharpCompilation.Create("compilation",
                [syntaxTree],
                metadataReferences,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

foreach (var item in compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error))
{
    Console.WriteLine(item.GetMessage());
}

var vectorGenerator = new VectorStructSourceGenerator();
var bindGroupGenerator = new BindGroupSourceGenerator();

GeneratorDriver driver = CSharpGeneratorDriver.Create(vectorGenerator, bindGroupGenerator);

driver = driver.RunGenerators(compilation!);
driver = driver.RunGeneratorsAndUpdateCompilation(compilation!, out var outputCompilation, out var diagnostics);

//if (Directory.Exists("sourceout"))
//{
//    Directory.GetFiles("sourceout").ToList().ForEach(f => File.Delete(f));
//}
//else
//{
//    Directory.CreateDirectory("sourceout");
//}

foreach (var item in outputCompilation.SyntaxTrees.Where(t => !string.IsNullOrEmpty(t.FilePath)))
{
    //File.WriteAllText($@"sourceout\{Path.GetFileName(item.FilePath)}", item.GetText().ToString());
    await CodeConsole.Write(item.GetText().ToString());
}


Console.WriteLine();