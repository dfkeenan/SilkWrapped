using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SilkWrapped.SourceGenerator;
[Generator(LanguageNames.CSharp)]
public class ObjectModelSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var splitChars = new[] { ';' };
        var config = context.GetMSBuildProperties(o => new GeneratorOptions
        {
            RootNamespace = o.GetMSBuildProperty("RootNamespace"),
            ApiTypeName = o.GetMSBuildProperty("SilkObjectModel_API"),
            ExtensionTypeNames = o.GetMSBuildProperty("SilkObjectModel_Extensions").Split(splitChars, StringSplitOptions.RemoveEmptyEntries),
            WrapperNameFormatString = o.GetMSBuildProperty("SilkObjectModel_WrapperNameFormatString", "{0}Wrapper"),
            ConstructionMethodNamePattern = new Regex(o.GetMSBuildProperty("SilkObjectModel_ConstructionMethodNamePattern", ".*(Create|Finish).*"), RegexOptions.Compiled),
            DisposalMethodNamePattern = new Regex(o.GetMSBuildProperty("SilkObjectModel_DisposalMethodNamePattern", ".*(Destroy|Release).*"), RegexOptions.Compiled),
        });
        //var x = context.MetadataReferencesProvider.Select((m,ct) => m.)

        var compilation = context.CompilationProvider
            .Combine(config);



        context.RegisterSourceOutput(compilation, (spc, compilation) =>
        {
            foreach (var item in new ObjectModelGenerator(compilation.Left, spc.CancellationToken, compilation.Right))
            {
                spc.AddSource($"{item.Name}.g.s", item.Source);
            }
        });
    }
}
