using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
//#if DEBUG 
//        if (!Debugger.IsAttached)
//        {
//            Debugger.Launch();
//        }
//#endif

        var config = context.GetMSBuildProperties(o => new GeneratorOptions
        {
            RootNamespace = o.GetMSBuildProperty("RootNamespace"),
            ApiTypeName = o.GetMSBuildProperty("SilkObjectModel_API"),
            ApiOwnerTypeName = o.GetMSBuildProperty("SilkObjectModel_APIOwnerTypeName"),
            ExtensionTypeNames = o.GetMSBuildProperty("SilkObjectModel_Extensions"),
            WrapperNameFormatString = o.GetMSBuildProperty("SilkObjectModel_WrapperNameFormatString", "{0}Wrapper"),
            ConstructionMethodNamePattern = o.GetMSBuildProperty("SilkObjectModel_ConstructionMethodNamePattern", ".*(Create|Finish|Acquire).*"),
            DisposalMethodNamePattern = o.GetMSBuildProperty("SilkObjectModel_DisposalMethodNamePattern", ".*(Release|Drop|Destroy).*"),
            HandleTypeNameExclusionPattern = o.GetMSBuildProperty("SilkObjectModel_HandleTypeNameExclusionPattern", "(Pfn).*|.*Descriptor"),
        });
        //var x = context.MetadataReferencesProvider.Select((m,ct) => m.)

        var compilation = context.CompilationProvider
            .WithComparer(CompilationReferencesEqualityComperer.Instance)
            .Combine(config);
            //.Select((s,ct) => new ObjectModelGenerator(s.Left, ct, s.Right));



        context.RegisterSourceOutput(compilation, (spc, compilation) =>
        {
            foreach (var item in new ObjectModelGenerator(compilation.Left, spc.CancellationToken, compilation.Right))
            {
                spc.AddSource($"{item.Name}.g.cs", item.Source);
            }
        });
    }
}

public class CompilationReferencesEqualityComperer : IEqualityComparer<Compilation>
{
    public static CompilationReferencesEqualityComperer Instance { get; } = new CompilationReferencesEqualityComperer();

    public bool Equals(Compilation x, Compilation y)
    {
        if (x == y) return true;

        return x.References.SequenceEqual(y.References);
    }

    public int GetHashCode(Compilation obj)
    {
        var hashCode = new HashCode();

        foreach (var x in obj.References)
            hashCode.Add(x.GetHashCode());

        return hashCode.ToHashCode();
    }
}