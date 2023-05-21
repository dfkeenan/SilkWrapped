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
    public string ApiOwnerTypeName { get; set; } = default!;
    public string ExtensionTypeNames { get; set; } = default!;
    public string WrapperNameFormatString { get; set; } = default!;
    public string ConstructionMethodNamePattern { get; set; } = default!;
    public string DisposalMethodNamePattern { get; set; } = default!;
    public string HandleTypeNameExclusionPattern { get; set; } = default!;
}


internal class ObjectModelGenerator : IEnumerable<(string Name, string Source)>
{
    public ObjectModelGenerator(Compilation compilation, CancellationToken cancellationToken, GeneratorOptions options)
    {
        this.rootNamespace = string.IsNullOrEmpty(options.RootNamespace) ? null : NamespaceDeclaration(ParseName(options.RootNamespace));
        apiType = string.IsNullOrEmpty(options.ApiTypeName) ? null : compilation.GetTypeByMetadataName(options.ApiTypeName);
        apiOwnerType = string.IsNullOrEmpty(options.ApiOwnerTypeName) ? null : compilation.GetTypeByMetadataName(options.ApiOwnerTypeName);
        
        disposeMethodPriority = new Dictionary<ITypeSymbol, int>(SymbolEqualityComparer.Default);

        if (options.ExtensionTypeNames is { Length: > 0 })
        {
            var splitChars = new[] { ';' };
            extensionTypes = options.ExtensionTypeNames
                .Split(splitChars, StringSplitOptions.RemoveEmptyEntries)
                .Select(n => compilation.GetTypeByMetadataName(n)!)
                .Where(t => t is not null)
                .ToArray();
        }

        this.cancellationToken = cancellationToken;
        wrapperNameFormatString = options.WrapperNameFormatString;
        constructionMethodNamePattern = new Regex( options.ConstructionMethodNamePattern, RegexOptions.Compiled);
        disposalMethodNamePattern = new Regex(options.DisposalMethodNamePattern, RegexOptions.Compiled);
        handleTypeNameExclusionPattern = new Regex(options.HandleTypeNameExclusionPattern, RegexOptions.Compiled);

        int priority = 0;
        if (extensionTypes is not null)
        {
            foreach (var item in extensionTypes)
            {
                disposeMethodPriority[item] = priority++;
            }
        }
        if (apiType != null)
        {
            disposeMethodPriority[apiType] = priority++;
        }
    }

    private NamespaceDeclarationSyntax? rootNamespace;
    private ITypeSymbol? apiType;
    private ITypeSymbol? apiOwnerType;
    private ITypeSymbol[]? extensionTypes;
    private string wrapperNameFormatString;
    private Regex constructionMethodNamePattern;
    private Regex disposalMethodNamePattern;
    private Regex handleTypeNameExclusionPattern;
    private readonly MethodSymbolGroupCollection constructionMethods = new();
    private readonly MethodSymbolGroupCollection methods = new();
    private readonly MethodSymbolGroupCollection disposalMethods = new();
    private readonly HashSet<ITypeSymbol> handleTypes = new (SymbolEqualityComparer.Default);
    private readonly CancellationToken cancellationToken;
    Dictionary<ITypeSymbol, int> disposeMethodPriority;

    private static readonly ArgumentSyntax handleArgument = Argument(IdentifierName("Handle"));
    private static readonly VariableDeclarationSyntax resultVariable = VariableDeclaration(
                                                    IdentifierName(
                                                        Identifier(
                                                            TriviaList(),
                                                            SyntaxKind.VarKeyword,
                                                            "var",
                                                            "var",
                                                            TriviaList())))
                                                                .WithVariables(
                                                                    SingletonSeparatedList(
                                                                        VariableDeclarator(
                                                                            Identifier("result"))));
    private static readonly ReturnStatementSyntax returnResultStatement = ReturnStatement(IdentifierName("result"));
    private static readonly ArgumentListSyntax wrapperCreationArgumentList = ArgumentList(
                            SeparatedList<ArgumentSyntax>(
                                new SyntaxNodeOrToken[]{
                                    Argument(
                                        IdentifierName("Api")),
                                    Token(SyntaxKind.CommaToken),
                                    Argument(
                                        IdentifierName("result"))}));

    private static readonly StatementSyntax returnIfNull = ParseStatement("if (result == null) return null;");

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
                                .AddDefaultConstructor(b => b.AddAssignmentStatement("Core", ParseExpression(apiType!.ToDisplayString() + ".GetApi()")))

                                .AddMembers(apiProperty)
                                .AddMembers(extensionTypes.Select(e => PropertyDeclaration(e, e.Name, SyntaxKind.PublicKeyword).WithSetter()))
                                .AddDispose(b => b.AddStatements(extensionTypes.Select(e => InvokeDispose(e.Name, true)).Append(InvokeDispose("Core"))));
        return apiContainer;
    }

    private ClassDeclarationSyntax GetWrapper(ITypeSymbol handleType)
    {
        var wrapperName = GetWrapperName(handleType);
        var name = GetName(handleType);
        var isApiOwner = IsAPIOwner(handleType);
        var members = new List<MemberDeclarationSyntax>();

        var constructionMethods = this.constructionMethods[handleType];

        var contructorMethodStatements = new SyntaxList<StatementSyntax>();

        if(isApiOwner)
        {
            contructorMethodStatements = contructorMethodStatements
                .Add(AssignmentStatement("Api", ParseExpression("new ApiContainer()")));
            //TODO: make API agnostic
            IEnumerable<StatementSyntax> extensionAssign = extensionTypes
                .Select(e => ParseStatement($"if(Api.Core.TryGetDeviceExtension(null, out {e.ToDisplayString()} {CamelCase(e.Name)})) Api.{e.Name} = {CamelCase(e.Name)};"));
            contructorMethodStatements = contructorMethodStatements.AddRange(extensionAssign);
        }



        foreach (var methodSymbol in SafetyFilter(constructionMethods))
        {
            var parameters = from parameterSymbol in methodSymbol.Parameters
                             let parameter = Parameter(parameterSymbol)
                             select IsHandleType(parameterSymbol.Type) ?
                                    parameter.WithType(IdentifierName(GetWrapperName(parameterSymbol.Type))) :
                                    parameter;

            IEnumerable<ArgumentSyntax> arguments;


            if (IsHandleType(methodSymbol.Parameters[0].Type))
            {
                contructorMethodStatements = contructorMethodStatements
                    .Add(AssignmentStatement("Api", 
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, 
                            IdentifierName(methodSymbol.Parameters[0].Name),
                            IdentifierName("Api"))));

                arguments = from parameterSymbol in methodSymbol.Parameters.Skip(1)
                            let argument = Argument(parameterSymbol)
                            select argument;

                arguments = arguments.Prepend(Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(methodSymbol.Parameters[0].Name),
                            IdentifierName("Handle"))));
            }
            else
            {
                arguments = from parameterSymbol in methodSymbol.Parameters
                            let argument = Argument(parameterSymbol)
                            select argument;
            }

            var apiMember = MethodApiMemberExpression(methodSymbol);

            var invocationExpression = InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, apiMember, IdentifierName(methodSymbol.Name)))
                                            .WithArgumentList(arguments);

            contructorMethodStatements = contructorMethodStatements.Add(AssignmentStatement("Handle", invocationExpression));


            var constructorDeclaration = ConstructorDeclaration(wrapperName)
                                            .WithModifiers(SyntaxKind.PublicKeyword)
                                            .WithParameterList(parameters)
                                            .WithBody(Block().AddStatements(contructorMethodStatements));

            members.Add(constructorDeclaration);
        }

        

        foreach (var methodSymbol in SafetyFilter(methods[handleType]))
        {
            var parameters = from parameterSymbol in methodSymbol.Parameters.Skip(1)
                             let parameter = Parameter(parameterSymbol)
                             select parameter;

            var arguments = from parameterSymbol in methodSymbol.Parameters.Skip(1)
                            let argument = Argument(parameterSymbol)
                            select argument;

            var methodDeclaration = MethodDeclaration(ReturnTypeSyntax(methodSymbol), methodSymbol.Name.Replace(name, ""))
                                        .WithParameterList(parameters)
                                        .WithModifiers(SyntaxKind.PublicKeyword);

            if(methodSymbol.IsGenericMethod)
            {
                var typeParameters = from typeParameter in methodSymbol.TypeParameters
                                     select TypeParameter(typeParameter);

                var constraints = from typeParameter in methodSymbol.TypeParameters
                                  let clause = TypeParameterConstraintClause(typeParameter)
                                  where clause.Constraints.Any()
                                  select clause;
                

                methodDeclaration = methodDeclaration
                    .WithTypeParameterList(TypeParameterList(new SeparatedSyntaxList<TypeParameterSyntax>().AddRange(typeParameters)))
                    .WithConstraintClauses(new SyntaxList<TypeParameterConstraintClauseSyntax>().AddRange(constraints));
            }

            BlockSyntax body = Block();

            var apiMember = MethodApiMemberExpression(methodSymbol);

            var invocationExpression = InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, apiMember, IdentifierName(methodSymbol.Name)))
                                            .WithArgumentList(arguments.Prepend(handleArgument));

            //if (methodSymbol.IsGenericMethod)
            //{
                
            //}

            if (methodSymbol.ReturnsVoid)
            {
                body = body.AddStatements(ExpressionStatement(invocationExpression));
            }
            else
            {
                ReturnStatementSyntax returnStatement;

                if(IsHandleType(methodSymbol.ReturnType))
                {
                    returnStatement = ReturnStatement(ObjectCreationExpression(IdentifierName(GetWrapperName(methodSymbol.ReturnType)))
                                        .WithArgumentList(wrapperCreationArgumentList));
                    body = body.AddStatements(LocalDeclarationStatement(resultVariable.WithInitializer(invocationExpression)), returnIfNull, returnStatement);
                }
                else
                {
                    returnStatement = returnResultStatement;
                    body = body.AddStatements(LocalDeclarationStatement(resultVariable.WithInitializer(invocationExpression)), returnStatement);
                }
            }


            members.Add(methodDeclaration.WithBody(body));
        }


        var apiProperty = PropertyDeclaration("ApiContainer", "Api", SyntaxKind.PublicKeyword);
        var handleProperty = PropertyDeclaration(handleType, "Handle", SyntaxKind.PublicKeyword);
        var declaration = ClassDeclaration(wrapperName, SyntaxKind.PublicKeyword, SyntaxKind.UnsafeKeyword, SyntaxKind.PartialKeyword)
                            .AddMembers(apiProperty, handleProperty)
                            .AddConstructor(apiProperty, handleProperty)
                            .AddMembers(members)
                            .AddMembers(ConversionOperatorDeclaration(Token(SyntaxKind.ImplicitKeyword), TypeSyntax(handleType))
                                            .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
                                            .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier(CamelCase(wrapperName))).WithType(IdentifierName(wrapperName)))))
                                            .WithExpressionBody(
                                                    ArrowExpressionClause(
                                                    MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        IdentifierName(CamelCase(wrapperName)), IdentifierName("Handle"))))
                                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

        var disposalMethods = this.disposalMethods[handleType];

        if(disposalMethods.Any())
        {
            var disposeMethodStatements = new SyntaxList<StatementSyntax>();

            foreach (var disposalMethod in disposalMethods.OrderByPriority(disposeMethodPriority))
            { 
                string statement;
                string apiMember = MethodApiMember(disposalMethod);

                if(Equals(apiType!, disposalMethod.ContainingType))
                {
                    statement = $"{apiMember}.{disposalMethod.Name}(Handle);";
                }
                else
                {
                    statement = $"if ({apiMember} != null) {apiMember}.{disposalMethod.Name}(Handle);";
                }

                if(disposeMethodStatements.Count > 0)
                {
                    statement = "else " + statement;
                }

                disposeMethodStatements = disposeMethodStatements.Add(ParseStatement(statement));

            }


            if (isApiOwner)
            {
                disposeMethodStatements = disposeMethodStatements.Add(ParseStatement("Api.Dispose();"));
            }

            declaration = declaration.AddDispose(b => b.AddStatements(disposeMethodStatements));
        }

        return declaration;
    }

    private ExpressionSyntax MethodApiMemberExpression(IMethodSymbol methodSymbol)
    {
        return ParseExpression(MethodApiMember(methodSymbol));
    }

    private string MethodApiMember(IMethodSymbol method)
    {
        if (Equals(apiType!, method.ContainingType))
        {
            return "Api.Core";
        }
        else
        {
            return $"Api.{method.ContainingType.Name}";
        }
    }

    private TypeSyntax ReturnTypeSyntax(IMethodSymbol methodSymbol)
    {
        if (methodSymbol.ReturnsVoid)
            return PredefinedType(Token(SyntaxKind.VoidKeyword));

        if(IsHandleType(methodSymbol.ReturnType))
            return ParseTypeName(GetWrapperName(methodSymbol.ReturnType));

        return TypeSyntax(methodSymbol.ReturnType);
    }

    private IEnumerable<IMethodSymbol> SafetyFilter(IEnumerable<IMethodSymbol> list)
    {
        foreach (var item in list)
        {
            var parameters = item.Parameters.AsEnumerable();

            if (IsHandleType(item.Parameters[0].Type))
                parameters = parameters.Skip(1);

            if (parameters.Any(p => p.Type is IPointerTypeSymbol && !IsHandleType(p.Type)))
                continue;


            yield return item;
        }
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
            
            if (IsDisposalMethod(methodSymbol))
            {
                disposalMethods.Add(firstParamType, methodSymbol);
                if (IsHandleType(typeSymbol, firstParamType))
                    handleTypes.Add(firstParamType);
            }
            else
            {
                methods.Add(firstParamType, methodSymbol);
                if (IsHandleType(typeSymbol, firstParamType))
                    handleTypes.Add(firstParamType);
            }

        }

        foreach (var methodSymbol in methodSymbols)
        {
            if (cancellationToken.IsCancellationRequested) return;

            if (methodSymbol.ReturnsVoid) continue;

            var returnType = methodSymbol.ReturnType;

            if (returnType is null) continue;

            if (IsHandleType(typeSymbol, returnType))
                handleTypes.Add(methodSymbol.ReturnType);
        }
    }

    private bool IsAPIOwner(ITypeSymbol typeSymbol)
    {
        if(apiOwnerType == null) return false;
        if (typeSymbol is IPointerTypeSymbol pointerTypeSymbol)
        {
            typeSymbol = pointerTypeSymbol.PointedAtType;
        }
        return Equals(apiOwnerType, typeSymbol);
    }

    private bool IsConstructionMethod(IMethodSymbol method)
    {
        return !method.ReturnsVoid &&
                constructionMethodNamePattern.IsMatch(method.Name) &&
                IsAPIOwner(method.ReturnType); // Only create constructor for API owner.
    }
    private bool IsDisposalMethod(IMethodSymbol method)
    {
        return method.ReturnsVoid && disposalMethodNamePattern.IsMatch(method.Name);
    }

    private bool IsExcludedHandleType(ITypeSymbol typeSymbol)
    {
        return handleTypeNameExclusionPattern.IsMatch(GetName(typeSymbol));
    }

    private bool IsHandleType(ITypeSymbol typeSymbol)
        => handleTypes.Contains(typeSymbol);

    private bool IsHandleType(ITypeSymbol typeSymbol, ITypeSymbol handleTypeCandidate)
    {
        return IsStructure(handleTypeCandidate) && Equals(GetNamespace(handleTypeCandidate), GetNamespace(typeSymbol)) && !IsExcludedHandleType(handleTypeCandidate);
    }

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


