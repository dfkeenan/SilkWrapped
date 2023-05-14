using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static SilkWrapped.SourceGenerator.CustomSyntaxFactory;

namespace SilkWrapped.SourceGenerator;

internal record class GeneratorOptions
{
    public string RootNamespace { get; set; } = default!;
    public string ApiTypeName { get; set; } = default!;
    public string[] ExtensionTypeNames { get; set; } = default!;
    public string WrapperNameFormatString { get; set; } = default!;
    public Regex ConstructionMethodNamePattern { get; set; } = default!;
    public Regex DisposalMethodNamePattern { get; set; } = default!;
}


internal class ObjectModelGenerator : IEnumerable<(string Name, string Source)>
{
    public ObjectModelGenerator(Compilation compilation, CancellationToken cancellationToken, GeneratorOptions options)
    {
        this.rootNamespace = string.IsNullOrEmpty(options.RootNamespace) ? null : NamespaceDeclaration(ParseName(options.RootNamespace));
        apiType = string.IsNullOrEmpty(options.ApiTypeName) ? null : compilation.GetTypeByMetadataName(options.ApiTypeName);

        if (options.ExtensionTypeNames is { Length: > 0 })
        {
            extensionTypes = options.ExtensionTypeNames.Select(n => compilation.GetTypeByMetadataName(n)!).Where(t => t is not null).ToArray();
        }

        this.cancellationToken = cancellationToken;
        wrapperNameFormatString = options.WrapperNameFormatString;
        constructionMethodNamePattern = options.ConstructionMethodNamePattern;
        disposalMethodNamePattern = options.DisposalMethodNamePattern;
    }

    private NamespaceDeclarationSyntax? rootNamespace;
    private ITypeSymbol? apiType;
    private ITypeSymbol[]? extensionTypes;
    private string wrapperNameFormatString;
    private Regex constructionMethodNamePattern;
    private Regex disposalMethodNamePattern;
    private readonly MethodSymbolGroupCollection constructionMethods = new();
    private readonly MethodSymbolGroupCollection methods = new();
    private readonly MethodSymbolGroupCollection disposalMethods = new();
    private readonly HashSet<ITypeSymbol> handleTypes = new (SymbolEqualityComparer.Default);
    private readonly CancellationToken cancellationToken;

    public IEnumerator<(string Name, string Source)> GetEnumerator()
    {
        if (apiType is null) yield break;
        if (rootNamespace is null) yield break;
        if (cancellationToken.IsCancellationRequested) yield break;

        CollectTypeInformation();

        yield return GetOutput(GetApiContainer());

        foreach (var handleType in handleTypes)
        {
            yield return GetOutput(GetWrapper(handleType));
        }

    }

    private ClassDeclarationSyntax GetApiContainer()
    {
        PropertyDeclarationSyntax apiProperty = PropertyDeclaration(apiType!, "Core", SyntaxKind.PublicKeyword);
        var apiContainer = ClassDeclaration("ApiContainer", SyntaxKind.PublicKeyword)
                                .AddDefaultConstructor(b => b.AddAssignmentStatement("Core", ParseExpression(apiType.ToDisplayString() + ".GetApi()")))

                                .AddMembers(apiProperty)
                                .AddMembers(extensionTypes.Select(e => PropertyDeclaration(e, e.Name, SyntaxKind.PublicKeyword).WithSetter()).ToArray())
                                .AddDispose(b => b.AddStatements(extensionTypes.Select(e => InvokeDispose(e.Name, true)).Append(InvokeDispose("Core")).ToArray()));
        return apiContainer;
    }

    private ClassDeclarationSyntax GetWrapper(ITypeSymbol handleType)
    {
        var name = GetWrapperName(handleType);

        var constructionMethods = this.constructionMethods[handleType];
        //var isRootObject = constructionMethods.SelectMany(m => m.Parameters.Select(p => p.Type)).All(t => handleTypes.Contains(t) == false);


        foreach (var item in constructionMethods)
        {

        }


        var declaration = ClassDeclaration(name, SyntaxKind.PublicKeyword)
                            .AddMembers(PropertyDeclaration("ApiContainer", "Api"),
                                        PropertyDeclaration(handleType,"Handle", SyntaxKind.PublicKeyword))


                            .AddDispose(b => b)

                            .AddMembers(ConversionOperatorDeclaration(Token(SyntaxKind.ImplicitKeyword), TypeSyntax(handleType))
                                            .WithModifiers(SyntaxKind.PublicKeyword,SyntaxKind.StaticKeyword)
                                            .WithParameterList(ParameterList(SingletonSeparatedList<ParameterSyntax>(Parameter(Identifier(CamelCase(name))).WithType(IdentifierName(name)))))
                                            .WithExpressionBody(
                                                    ArrowExpressionClause(
                                                    MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        IdentifierName(CamelCase(name)), IdentifierName("Handle"))))
                                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)))
                            ;

        return declaration;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void CollectTypeInformation()
    {
        CollectTypeInformation(apiType!);
        if(extensionTypes is not null)
        {
            foreach (var extensionType in extensionTypes)
            {
                CollectTypeInformation(extensionType);
            }
        }
    }

    private void CollectTypeInformation(ITypeSymbol typeSymbol)
    {
        if(cancellationToken.IsCancellationRequested) return;

        var typeSymbols = typeSymbol.ContainingNamespace.GetMembers().OfType<ITypeSymbol>().Where(t => t.TypeKind == TypeKind.Struct).ToList();


        var methodSymbols = from method in typeSymbol.GetMembers().OfType<IMethodSymbol>()
                          where method.DeclaredAccessibility == Accessibility.Public &&
                                method.MethodKind == MethodKind.Ordinary &&
                                method.IsStatic == false &&
                                method.Parameters.Length > 0 &&
                                Equals(typeSymbol, method.ReturnType) == false
                            select method;

        foreach (var methodSymbol in methodSymbols)
        {
            if (cancellationToken.IsCancellationRequested) return;


            var firstParamType = methodSymbol.Parameters[0].Type;

            if(IsConstructionMethod(methodSymbol))
            {
                constructionMethods.Add(methodSymbol.ReturnType, methodSymbol);
            }
            else if (IsDisposalMethod(methodSymbol))
            {
                disposalMethods.Add(firstParamType, methodSymbol);
                if (IsStructure(firstParamType) && Equals(GetNamespace(firstParamType), GetNamespace(typeSymbol)))
                    handleTypes.Add(firstParamType);
            }
            else
            {
                methods.Add(firstParamType, methodSymbol);
                if (IsStructure(firstParamType) && Equals(GetNamespace(firstParamType), GetNamespace(typeSymbol)))
                    handleTypes.Add(firstParamType);
            }

        }

        foreach (var methodSymbol in methodSymbols)
        {
            if (cancellationToken.IsCancellationRequested) return;

            if (methodSymbol.ReturnsVoid) continue;

            var returnType = methodSymbol.ReturnType;

            if (returnType is null) continue;


            if (Equals(GetNamespace(returnType), GetNamespace(typeSymbol)) == false)
                continue;

            if (constructionMethods.ContainsKey(methodSymbol.ReturnType))
                handleTypes.Add(methodSymbol.ReturnType);
        }
    }

    private bool IsConstructionMethod(IMethodSymbol method)
    {
        return !method.ReturnsVoid &&
                constructionMethodNamePattern.IsMatch(method.Name);
    }
    private bool IsDisposalMethod(IMethodSymbol method)
    {
        return method.ReturnsVoid && disposalMethodNamePattern.IsMatch(method.Name);
    }

    private bool IsHandleType(ITypeSymbol typeSymbol)
        => handleTypes.Contains(typeSymbol);

    private INamespaceSymbol GetNamespace(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is IPointerTypeSymbol pointerTypeSymbol)
        {
            typeSymbol = pointerTypeSymbol.PointedAtType;
        }

        return typeSymbol.ContainingNamespace;
    }

    private bool IsStructure(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is IPointerTypeSymbol pointerTypeSymbol)
        {
            typeSymbol = pointerTypeSymbol.PointedAtType;
        }

        return typeSymbol.TypeKind == TypeKind.Structure;
    }

    private bool Equals(ISymbol symbol, ISymbol other)
    {
        return symbol.Equals(other, SymbolEqualityComparer.Default);
    }

    private string GetName(ITypeSymbol typeSymbol)
    {
        var nameType = typeSymbol;
        if (nameType is IPointerTypeSymbol pointerTypeSymbol)
        {
            nameType = pointerTypeSymbol.PointedAtType;
        }

        return nameType.Name;
    }

    private string GetWrapperName(ITypeSymbol typeSymbol)
        => string.Format(wrapperNameFormatString, GetName(typeSymbol));

    private (string Name, string Source) GetOutput(TypeDeclarationSyntax typeDeclaration)
    {
        return (typeDeclaration.Identifier.Text, rootNamespace!.WithMembers(new SyntaxList<MemberDeclarationSyntax>(typeDeclaration)).NormalizeWhitespace().ToFullString());
    }
}


