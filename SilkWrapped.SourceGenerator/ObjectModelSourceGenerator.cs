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
    internal const string ApiExtensionAttributeName = "ApiExtensionAttribute";

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
                    public class ApiContainerAttribute : Attribute
                    {
                        public ApiContainerAttribute(Type apiType, Type apiOwnerType)
                        {
                            ApiType = apiType;
                            ApiOwnerType = apiOwnerType;
                        }

                        public Type ApiType { get; }
                        public Type ApiOwnerType { get; set; }
                        public string WrapperNameFormatString { get; set; }
                        public string ConstructionMethodNamePattern { get; set; }
                        public string DisposalMethodNamePattern { get; set; }
                        public string HandleTypeNameExclusionPattern { get; set; }
                    }

                    [Conditional("{{conditonString}}")]
                    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
                    public class {{ApiExtensionAttributeName}} : Attribute
                    {
                        public ApiExtensionAttribute(Type extensionType)
                        {
                            ExtensionType = extensionType;
                        }
                
                        public Type ExtensionType { get; }
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

        var apiContainer = context.SyntaxProvider.ForAttributeWithMetadataName<(INamedTypeSymbol? containerType, ITypeSymbol? apiType, ITypeSymbol? apiOwnerType, GeneratorOptions? options)>($"{Namespace}.ApiContainerAttribute",
           (node, ct) =>
           {
               return node is ClassDeclarationSyntax;
           },
           (context, ct) =>
           {
               var containerType = context.TargetSymbol as INamedTypeSymbol;
               GeneratorOptions? options = null;

               if (context.Attributes.Length == 1 && context.Attributes[0].ConstructorArguments.Length == 2)
               {
                   var apiType = context.Attributes[0].ConstructorArguments[0].Value as ITypeSymbol;
                   var apiOwnerType = context.Attributes[0].ConstructorArguments[1].Value as ITypeSymbol;

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


                   return (containerType, apiType, apiOwnerType, options);
               }

               return (containerType, null, null, null);
           })
            .Where(o => o.apiType is not null);


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



        context.RegisterSourceOutput(apiContainer, (spc, compilation) =>
        {
            foreach (var item in new ObjectModelGenerator(compilation.containerType!, compilation.apiType!, compilation.apiOwnerType!, compilation.options!, spc.CancellationToken))
            {
                spc.AddSource($"{item.Name}.g.cs", item.Source);
            }
        });
    }
}