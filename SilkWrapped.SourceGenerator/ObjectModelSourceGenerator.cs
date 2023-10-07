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
    private const string Namespace = "SilkWrapped.SourceGenerator";
    private const string conditonString = "SILKWRAPPEDSOURCEGENERATOR";
    private const string ReplaceMethodAttributeName = "ReplaceMethodAttribute";
    private const string ApiContainerAttributeName = "ApiContainerAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //#if DEBUG 
        //        if (!Debugger.IsAttached)
        //        {
        //            Debugger.Launch();
        //        }
        //#endif

        context.RegisterPostInitializationOutput(context =>
        {
            context.AddSource($"{Namespace}.ApiAttributes.g.cs",
                $$""""
                using System;
                using System.Diagnostics;

                namespace {{Namespace}}
                {
                    [Conditional("{{conditonString}}")]
                    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
                    public class {{ApiContainerAttributeName}} : Attribute
                    {
                        public {{ApiContainerAttributeName}}(Type apiOwnerType)
                        {
                            ApiOwnerType = apiOwnerType;
                        }

                        public Type ApiOwnerType { get; set; }
                        public string WrapperNameFormatString { get; set; }
                        public string ConstructionMethodNamePattern { get; set; }
                        public string DisposalMethodNamePattern { get; set; }
                        public string HandleTypeNameExclusionPattern { get; set; }
                    }

                    [Conditional("{{conditonString}}")]
                    [AttributeUsage(AttributeTargets.Method)]
                    public class {{ReplaceMethodAttributeName}} : Attribute
                    {
                        public {{ReplaceMethodAttributeName}}(string methodName)
                        {
                            MethodName = methodName;
                        }
                
                        public string MethodName { get; }
                    }
                }
                """");
        });

        var apiContainer = context.SyntaxProvider.ForAttributeWithMetadataName<(INamedTypeSymbol? containerType, ITypeSymbol? apiOwnerType, GeneratorOptions? options)>($"{Namespace}.{ApiContainerAttributeName}",
           (node, ct) =>
           {
               return node is ClassDeclarationSyntax cds && cds.IsPartial();
           },
           (context, ct) =>
           {
               var containerType = context.TargetSymbol as INamedTypeSymbol;
               GeneratorOptions? options = null;

               if (context.Attributes.Length == 1 && context.Attributes[0].ConstructorArguments.Length == 1)
               {
                   var apiOwnerType = context.Attributes[0].ConstructorArguments[0].Value as ITypeSymbol;

                   options = new GeneratorOptions();

                   foreach (var item in context.Attributes[0].NamedArguments)
                   {
                       if (item.Value.Value is not string value) continue;

                       switch (item.Key)
                       {
                           case nameof(GeneratorOptions.WrapperNameFormatString):
                               options.WrapperNameFormatString = value;
                               break;
                           case nameof(GeneratorOptions.ConstructionMethodNamePattern):
                               options.ConstructionMethodNamePattern = value;
                               break;
                           case nameof(GeneratorOptions.DisposalMethodNamePattern):
                               options.DisposalMethodNamePattern = value;
                               break;
                           case nameof(GeneratorOptions.HandleTypeNameExclusionPattern):
                               options.HandleTypeNameExclusionPattern = value;
                               break;

                       }
                   }


                   return (containerType, apiOwnerType, options);
               }

               return (containerType, null, null);
           })
            .Where(o => o.apiOwnerType is not null);

        var objectModel = apiContainer
            .Select((c, ct) =>
                new ObjectModelGenerator(c.containerType!, c.apiOwnerType!, c.options!));


        //var replaceAttributes = context.SyntaxProvider.ForAttributeWithMetadataName($"{Namespace}.{ReplaceMethodAttributeName}",
        //    (node, ct) =>
        //    {
        //        return node is MethodDeclarationSyntax;
        //    },
        //    (context, ct) =>
        //    {
        //        var containeringType = context.TargetSymbol.ContainingType.Name;
        //        var methodName = context.Attributes[0].ConstructorArguments[0].Value as string;

        //        return (containeringType, methodName);
        //    }).Combine(apiContainer);



        context.RegisterSourceOutput(objectModel, (spc, objectModel) =>
        {
            foreach (var item in objectModel.GetSources(spc.CancellationToken))
            {
                spc.AddSource($"{item.Name}.g.cs", item.Source);
            }
        });
    }
}