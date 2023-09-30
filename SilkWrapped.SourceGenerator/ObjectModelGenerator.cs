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
    public string WrapperNameFormatString { get; set; } = "{0}Wrapper";
    public string ConstructionMethodNamePattern { get; set; } = ".*(Create|Finish|Acquire).*";
    public string DisposalMethodNamePattern { get; set; } = ".*(Release|Drop|Destroy).*";
    public string HandleTypeNameExclusionPattern { get; set; } = "(Pfn).*|.*Descriptor";
}


internal class ObjectModelGenerator : IEquatable<ObjectModelGenerator>
{
    public ObjectModelGenerator(INamedTypeSymbol containerType, ITypeSymbol apiOwnerType, GeneratorOptions options, INamedTypeSymbol nativeApiType, INamedTypeSymbol extensionAttributeType)
    {

        rootNamespace = NamespaceDeclaration(ParseName(containerType.ContainingNamespace.ToString()));
        this.containerType = containerType;
        this.apiOwnerType = apiOwnerType;

        extensionTypes = new List<ITypeSymbol>(); 
        extensionProperties = new List<IPropertySymbol>();

        foreach (var member in containerType.GetMembers())
        {
            if (member is not IPropertySymbol property) continue;
            //property.ty
            if (property.Type.Is(nativeApiType))
            {
                apiType = property.Type.WithNullableAnnotation(NullableAnnotation.None);
                apiProperty = property;
                continue;
            }

            if(property.Type.GetAttributes().Any(a => a.AttributeClass?.Is(extensionAttributeType) is true))
            {
                extensionTypes.Add(property.Type.WithNullableAnnotation(NullableAnnotation.None));
                extensionProperties.Add(property);
            }
        }

        wrapperNameFormatString = options.WrapperNameFormatString;
        constructionMethodNamePattern = new Regex(options.ConstructionMethodNamePattern, RegexOptions.Compiled);
        disposalMethodNamePattern = new Regex(options.DisposalMethodNamePattern, RegexOptions.Compiled);
        handleTypeNameExclusionPattern = new Regex(options.HandleTypeNameExclusionPattern, RegexOptions.Compiled);


        disposeMethodPriority = new Dictionary<ITypeSymbol, int>(SymbolEqualityComparer.Default);

        int priority = 0;
        foreach (var item in extensionTypes)
        {
            disposeMethodPriority[item] = priority++;
        }

        if (apiType != null)
        {
            disposeMethodPriority[apiType] = priority++;
        }
    }

    private NamespaceDeclarationSyntax? rootNamespace;
    private readonly INamedTypeSymbol containerType;
    private ITypeSymbol? apiType;
    private IPropertySymbol? apiProperty;
    private ITypeSymbol apiOwnerType;
    private List<ITypeSymbol> extensionTypes;
    private List<IPropertySymbol> extensionProperties;
    private string wrapperNameFormatString;
    private Regex constructionMethodNamePattern;
    private Regex disposalMethodNamePattern;
    private Regex handleTypeNameExclusionPattern;
    private readonly MethodSymbolGroupCollection constructionMethods = new();
    private readonly MethodSymbolGroupCollection methods = new();
    private readonly MethodSymbolGroupCollection disposalMethods = new();
    private readonly HashSet<ITypeSymbol> handleTypes = new (SymbolEqualityComparer.Default);
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

    public IEnumerable<(string Name, string Source)> GetSources(CancellationToken cancellationToken)
    {
        if (apiType is null) yield break;
        if (apiProperty is null) yield break;
        if (rootNamespace is null) yield break;
        if (cancellationToken.IsCancellationRequested) yield break;

        CollectTypeInformation(cancellationToken);

        yield return GetOutput(GetApiContainer());

        foreach (var handleType in handleTypes)
        {
            yield return GetOutput(GetWrapper(handleType));
        }

    }

    private ClassDeclarationSyntax GetApiContainer()
    {
        var apiContainer = ClassDeclaration(containerType.Name, SyntaxKind.PublicKeyword, SyntaxKind.PartialKeyword)
                                .AddDispose(b => b.AddStatements(extensionProperties.Select(e => InvokeDispose(e.Name, true)).Append(InvokeDispose(apiProperty!.Name))));
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
                .Add(AssignmentStatement("Api", ParseExpression($"new {containerType.Name}()")));
        }

        foreach (var methodSymbol in SafetyFilter(constructionMethods))
        {
            var parameters = from parameterSymbol in methodSymbol.Parameters
                             select GetParameter(parameterSymbol);

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
                             select GetParameter(parameterSymbol);

            var arguments = from parameterSymbol in methodSymbol.Parameters.Skip(1)
                            select GetArgument(parameterSymbol);

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

            foreach (var parameterSymbol in methodSymbol.Parameters.Skip(1))
            {
                if (IsHandleType(parameterSymbol.Type) is false) continue;

                body = body.AddStatements(ParseStatement($"var {parameterSymbol.Name}Ref = {parameterSymbol.Name}.Handle;"));
            }


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


        var apiProperty = PropertyDeclaration(containerType.Name, "Api", SyntaxKind.PublicKeyword);
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
            return $"Api.{apiProperty!.Name}";
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

    private void CollectTypeInformation(CancellationToken cancellationToken)
    {
        CollectTypeInformation(apiType!, cancellationToken);

        foreach (var extensionType in extensionTypes)
        {
            CollectTypeInformation(extensionType, cancellationToken);
        }
    }

    private void CollectTypeInformation(ITypeSymbol typeSymbol, CancellationToken cancellationToken)
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


    private ArgumentSyntax GetArgument(IParameterSymbol parameterSymbol)
    {
        ArgumentSyntax argument = Argument(parameterSymbol);

        if (IsHandleType(parameterSymbol.Type))
        {
            return argument.WithExpression(IdentifierName($"{parameterSymbol.Name}Ref"));
        }

        return argument;
    }

    private ParameterSyntax GetParameter(IParameterSymbol parameterSymbol)
    {
        ParameterSyntax parameter = Parameter(parameterSymbol, IsHandleType(parameterSymbol.Type));


        if (IsHandleType(parameterSymbol.Type))
        {
            return parameter.WithType(IdentifierName(GetWrapperName(parameterSymbol.Type)));
        } 

        return DefaultValueIfApplicable(parameter, parameterSymbol);
    }

    private ParameterSyntax DefaultValueIfApplicable(ParameterSyntax parameter, IParameterSymbol parameterSymbol)
    {
        if(parameterSymbol.RefKind is not ( RefKind.None or RefKind.In )) return parameter;
        if(parameterSymbol.Type.IsValueType is false) return parameter;

        //Not sure about this one
        if(parameterSymbol.Type.Name.EndsWith("Descriptor") is false) return parameter;

        var shouldDefault = true;

        foreach (var member in parameterSymbol.Type.GetMembers())
        {
            if (member is not IFieldSymbol fieldSymbol) continue;
            if(fieldSymbol.IsStatic) continue;

            if(fieldSymbol.Name == "NextInChain" && fieldSymbol.Type.ToString().EndsWith("ChainedStruct*")) continue;
            if(fieldSymbol.Name == "Label" && fieldSymbol.Type.ToString().EndsWith("byte*")) continue;

            shouldDefault = false;
            break;
        }

        if (shouldDefault) return parameter.WithDefault(EqualsValueClause(
                                LiteralExpression(
                                    SyntaxKind.DefaultLiteralExpression,
                                    Token(SyntaxKind.DefaultKeyword))));

        return parameter;
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

    public bool Equals(ObjectModelGenerator other)
    {
        if(ReferenceEquals(other, null)) return false;

        if(!SymbolEqualityComparer.Default.Equals(apiOwnerType, other.apiOwnerType)) return false;
        if(!SymbolEqualityComparer.Default.Equals(containerType, other.containerType)) return false;
        if(!SymbolEqualityComparer.Default.Equals(apiType, other.apiType)) return false;
        if(!wrapperNameFormatString.Equals(other.wrapperNameFormatString)) return false;
        if(!constructionMethodNamePattern.Equals(other.constructionMethodNamePattern)) return false;
        if(!disposalMethodNamePattern.Equals(other.disposalMethodNamePattern)) return false;
        if(handleTypeNameExclusionPattern.Equals(other.handleTypeNameExclusionPattern)) return false;


        return true;
    }

    public override bool Equals(object obj)
    {
        return obj is ObjectModelGenerator g && Equals(g);
    }

    public override int GetHashCode() 
    {
        var hc = new HashCode();
        hc.Add(apiOwnerType, SymbolEqualityComparer.Default);
        hc.Add(containerType, SymbolEqualityComparer.Default);
        hc.Add(apiType, SymbolEqualityComparer.Default);
        hc.Add(wrapperNameFormatString);
        hc.Add(constructionMethodNamePattern);
        hc.Add(disposalMethodNamePattern);
        hc.Add(handleTypeNameExclusionPattern);
        return hc.ToHashCode();
    }
}


