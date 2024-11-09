using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SilkWrapped.SourceGenerator;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static SilkWrapped.SourceGenerator.CustomSyntaxFactory;

namespace SilkWrapped.ObjectModelTool;

internal record class GeneratorOptions
{
    public const string DefaultWrapperNameFormatString = "{0}Wrapper";
    public const string DefaultConstructionMethodNamePattern = ".*(Create|Finish|Acquire).*";
    public const string DefaultDisposalMethodNamePattern = ".*(Release|Drop|Destroy).*";
    public const string DefaultHandleTypeNameExclusionPattern = "(Pfn).*|.*Descriptor";

    public string WrapperNameFormatString        { get; set; } = DefaultWrapperNameFormatString;
    public string ConstructionMethodNamePattern  { get; set; } = DefaultConstructionMethodNamePattern;
    public string DisposalMethodNamePattern      { get; set; } = DefaultDisposalMethodNamePattern;
    public string HandleTypeNameExclusionPattern { get; set; } = DefaultHandleTypeNameExclusionPattern;
}


internal class ObjectModelGenerator : IEquatable<ObjectModelGenerator>
{
    public ObjectModelGenerator(INamedTypeSymbol containerType, ITypeSymbol apiOwnerType, GeneratorOptions options)
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
            if (property.Type.Is("Silk.NET.Core.Native.NativeAPI"))
            {
                apiType = property.Type.WithNullableAnnotation(NullableAnnotation.None);
                apiProperty = property;
                continue;
            }

            if (property.Type.GetAttributes().Any(a => a.AttributeClass?.Is("Silk.NET.Core.Attributes.ExtensionAttribute") is true))
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
    private readonly HashSet<ITypeSymbol> handleTypes = new(SymbolEqualityComparer.Default);
    Dictionary<ITypeSymbol, int> disposeMethodPriority;

    private readonly List<INamedTypeSymbol> delegateTypeSymbols = new();
    private readonly Dictionary<string, IMethodSymbol> delegateMethodSymbols = new();

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

    private static readonly VariableDeclarationSyntax resultWrapperVariable = VariableDeclaration(
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
                                                                            Identifier("resultWrapper"))));



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

        foreach (var delegateType in delegateTypeSymbols)
        {
            yield return GetOutput(GetDelegate(delegateType));
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

        var createdWrapperCallbackNames = new HashSet<string>();

        var constructionMethods = this.constructionMethods[handleType];

        var contructorMethodStatements = new SyntaxList<StatementSyntax>();

        if (isApiOwner)
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
                            let argument = GetArgument(parameterSymbol)
                            select argument;

                arguments = arguments.Prepend(Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(methodSymbol.Parameters[0].Name),
                            IdentifierName("Handle"))));
            }
            else
            {
                arguments = from parameterSymbol in methodSymbol.Parameters
                            let argument = GetArgument(parameterSymbol)
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

            if (ShouldAddDefaultParamOverride(methodSymbol, constructorDeclaration, out var overrideMethodDeclaration))
            {
                members.Add(overrideMethodDeclaration);
            }
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

            if (methodSymbol.IsGenericMethod)
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
                if (IsFuntionPointer(parameterSymbol.Type))
                {
                    var functionPointerParameters = GetFunctionPointerParameters(parameterSymbol.Type);
                    var callbackParameters = from parameter in functionPointerParameters
                                             select (Parameter(Identifier(parameter.Name)));

                    var callbackArguments = from parameter in functionPointerParameters
                                            select GetCallbackArgument(parameter, handleType);


                    body = body.AddStatements(LocalDeclarationStatement(
                        VariableDeclaration(TypeSyntax(parameterSymbol.Type))
                            .WithVariables(VariableDeclarator(Identifier($"{parameterSymbol.Name}Pfn"))
                                .WithInitializer(EqualsValueClause
                                    (ObjectCreationExpression(TypeSyntax(parameterSymbol.Type))
                                        .WithArgumentList(Argument(ParenthesizedLambdaExpression()
                                            .WithParameterList(callbackParameters)
                                                .WithBlock(Block()
                                                    .AddStatements(ExpressionStatement(
                                                        InvocationExpression(IdentifierName(parameterSymbol.Name))
                                                            .WithArgumentList(callbackArguments))))
                                            )))))));
                }
                else if (IsHandleType(parameterSymbol.Type))
                {
                    body = body.AddStatements(ParseStatement($"var {parameterSymbol.Name}Ref = {parameterSymbol.Name}.Handle;"));
                }

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

                if (IsHandleType(methodSymbol.ReturnType))
                {
                    string resultWrapperName = GetWrapperName(methodSymbol.ReturnType);
                    var resultWrapperDeclaration = LocalDeclarationStatement(resultWrapperVariable.WithInitializer(ObjectCreationExpression(IdentifierName(resultWrapperName))
                                        .WithArgumentList(wrapperCreationArgumentList)));
                    var createdCallbackName = $"{resultWrapperName}Created";
                    var createdCallback = ParseStatement($"{createdCallbackName}(resultWrapper);");
                    returnStatement = ReturnStatement(IdentifierName("resultWrapper"));
                    body = body.AddStatements(LocalDeclarationStatement(resultVariable.WithInitializer(invocationExpression)), returnIfNull, resultWrapperDeclaration, createdCallback, returnStatement);

                    if (createdWrapperCallbackNames.Add(createdCallbackName))
                    {
                        members.Add(ParseMemberDeclaration($"partial void {createdCallbackName}({resultWrapperName} value);")!);
                    }
                }
                else
                {
                    returnStatement = returnResultStatement;
                    body = body.AddStatements(LocalDeclarationStatement(resultVariable.WithInitializer(invocationExpression)), returnStatement);
                }
            }


            methodDeclaration = methodDeclaration.WithBody(body);
            members.Add(methodDeclaration);

            if (ShouldAddDefaultParamOverride(methodSymbol, methodDeclaration, out var overrideMethodDeclaration))
            {
                members.Add(overrideMethodDeclaration);
            }
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

        if (disposalMethods.Any())
        {
            var disposeMethodStatements = new SyntaxList<StatementSyntax>();
            disposeMethodStatements = disposeMethodStatements.Add(ParseStatement("if (Handle == default) return;"));
            disposeMethodStatements = disposeMethodStatements.Add(ParseStatement("Disposing();"));

            int ifCount = 0;

            foreach (var disposalMethod in disposalMethods.OrderByPriority(disposeMethodPriority))
            {
                string statement;
                string apiMember = MethodApiMember(disposalMethod);
                bool isIfStatement = false;

                if (Equals(apiType!, disposalMethod.ContainingType))
                {
                    statement = $"{apiMember}.{disposalMethod.Name}(Handle);";
                }
                else
                {
                    statement = $"if ({apiMember} != null) {apiMember}.{disposalMethod.Name}(Handle);";
                    isIfStatement = true;
                }

                if (ifCount > 0)
                {
                    statement = "else " + statement;
                }

                disposeMethodStatements = disposeMethodStatements.Add(ParseStatement(statement));

                if (isIfStatement)
                {
                    ifCount++;
                }
            }


            if (isApiOwner)
            {
                disposeMethodStatements = disposeMethodStatements.Add(ParseStatement("Api.Dispose();"));
            }


            disposeMethodStatements = disposeMethodStatements.Add(ParseStatement("Disposed();"));

            declaration = declaration.AddDispose(b => b.AddStatements(disposeMethodStatements))
                                     .AddMembers(ParseMemberDeclaration($"partial void Disposing();")!,
                                     ParseMemberDeclaration($"partial void Disposed();")!);
        }

        return declaration;
    }

    private DelegateDeclarationSyntax GetDelegate(INamedTypeSymbol delegateType)
    {
        IMethodSymbol methodSymbol = delegateType.DelegateInvokeMethod!;
        var parameters = from parameterSymbol in methodSymbol.Parameters
                         select GetParameter(parameterSymbol);

        return DelegateDeclaration(ReturnTypeSyntax(methodSymbol), Identifier(delegateType.Name))
                    .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.UnsafeKeyword)
                    .WithParameterList(parameters);
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

        if (IsHandleType(methodSymbol.ReturnType))
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
        if (cancellationToken.IsCancellationRequested) return;


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

            if (IsConstructionMethod(methodSymbol))
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

        var delegateTypeSymbols = from @delegate in typeSymbol.ContainingNamespace.GetMembers().OfType<INamedTypeSymbol>()
                                  where @delegate.TypeKind == TypeKind.Delegate &&
                                        @delegate.DeclaredAccessibility == Accessibility.Public
                                  select @delegate;

        foreach (var delegateTypeSymbol in delegateTypeSymbols)
        {
            this.delegateTypeSymbols.Add(delegateTypeSymbol);
            delegateMethodSymbols[delegateTypeSymbol.ToString()] = delegateTypeSymbol.DelegateInvokeMethod!;
        }

    }


    private ArgumentSyntax GetArgument(IParameterSymbol parameterSymbol)
    {
        ArgumentSyntax argument = Argument(parameterSymbol);

        if (IsFuntionPointer(parameterSymbol.Type))
        {
            return argument.WithExpression(IdentifierName($"{parameterSymbol.Name}Pfn"));
        }

        if (IsHandleType(parameterSymbol.Type))
        {
            return argument.WithExpression(IdentifierName($"{parameterSymbol.Name}Ref"));
        }

        if (parameterSymbol.RefKind == RefKind.RefReadOnlyParameter)
        {
            return argument.WithRefKindKeyword(Token(SyntaxKind.InKeyword));
        }

        return argument;
    }

    private ArgumentSyntax GetCallbackArgument(IParameterSymbol parameterSymbol, ITypeSymbol handeType)
    {
        ArgumentSyntax argument = Argument(parameterSymbol);

        if (IsHandleType(parameterSymbol.Type))
        {
            return Equals(parameterSymbol.Type, handeType)
                ? argument.WithExpression(ParseExpression($"{parameterSymbol.Name} == default ? null : (Handle == {parameterSymbol.Name} ? this : new {GetWrapperName(parameterSymbol.Type)}(Api, {parameterSymbol.Name}))"))
                : argument.WithExpression(ParseExpression($"{parameterSymbol.Name} == default ? null : (new {GetWrapperName(parameterSymbol.Type)}(Api, {parameterSymbol.Name}))"));
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

        if (IsFuntionPointer(parameterSymbol.Type))
        {
            return parameter.WithType(IdentifierName(parameterSymbol.Type.Name.Substring("Pfn".Length)));
        }

        return parameter;
    }

    private bool IsFuntionPointer(ITypeSymbol typeSymbol)
        => typeSymbol is { IsValueType: true, IsReadOnly: true } && typeSymbol.Name.StartsWith("Pfn");

    private IEnumerable<IParameterSymbol> GetFunctionPointerParameters(ITypeSymbol typeSymbol)
        => delegateMethodSymbols[typeSymbol.ToString().Replace("Pfn", "")].Parameters;


    private bool ShouldAddDefaultParamOverride<T>(IMethodSymbol methodSymbol, T declaration, out T? overrideDeclaration)
        where T : BaseMethodDeclarationSyntax
    {
        overrideDeclaration = null;
        if (!(methodSymbol.Parameters.Length <= 2 && ShouldDefaultValue(methodSymbol.Parameters.Last()))) return false;

        var defaultParameterSymbol = methodSymbol.Parameters.Last();
        var lastParam = declaration.ParameterList.Parameters.Last();


        var defaultValue = LocalDeclarationStatement(
                        VariableDeclaration(TypeSyntax(defaultParameterSymbol.Type))
                            .WithVariables(VariableDeclarator(lastParam.Identifier)
                                .WithInitializer(EqualsValueClause
                                    (ObjectCreationExpression(TypeSyntax(defaultParameterSymbol.Type))
                                        .WithArgumentList()))));

        var body = Block().AddStatements(declaration.Body!.Statements.Prepend(defaultValue));


        overrideDeclaration = (T)declaration.WithBody(body)
                                         .WithParameterList(Enumerable.Empty<ParameterSyntax>());

        return true;
    }


    private bool ShouldDefaultValue(IParameterSymbol parameterSymbol)
    {
        if (parameterSymbol.RefKind is not (RefKind.None or RefKind.In or RefKind.RefReadOnlyParameter)) return false;
        if (parameterSymbol.Type.IsValueType is false) return false;

        //Not sure about this one
        if (parameterSymbol.Type.Name.EndsWith("Descriptor") is false) return false;

        foreach (var member in parameterSymbol.Type.GetMembers())
        {
            if (member is not IFieldSymbol fieldSymbol) continue;
            if (fieldSymbol.IsStatic) continue;

            if (fieldSymbol.Name == "NextInChain" && fieldSymbol.Type.ToString().EndsWith("ChainedStruct*")) continue;
            if (fieldSymbol.Name == "Label" && fieldSymbol.Type.ToString().EndsWith("byte*")) continue;

            return false;
        }

        return true;
    }

    private bool IsAPIOwner(ITypeSymbol typeSymbol)
    {
        if (apiOwnerType == null) return false;
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

    private (string Name, string Source) GetOutput(MemberDeclarationSyntax memberDeclaration)
    {
        string name = memberDeclaration switch
        {
            TypeDeclarationSyntax t => t.Identifier.Text,
            DelegateDeclarationSyntax t => t.Identifier.Text,
            _ => Guid.NewGuid().ToString()
        };


        return (name, rootNamespace!.WithMembers(new SyntaxList<MemberDeclarationSyntax>(memberDeclaration)).NormalizeWhitespace().ToFullString());
    }

    public bool Equals(ObjectModelGenerator other)
    {
        if (ReferenceEquals(other, null)) return false;

        if (!SymbolEqualityComparer.Default.Equals(apiOwnerType, other.apiOwnerType)) return false;
        if (!SymbolEqualityComparer.Default.Equals(containerType, other.containerType)) return false;
        if (!SymbolEqualityComparer.Default.Equals(apiType, other.apiType)) return false;
        if (!wrapperNameFormatString.Equals(other.wrapperNameFormatString)) return false;
        if (!constructionMethodNamePattern.Equals(other.constructionMethodNamePattern)) return false;
        if (!disposalMethodNamePattern.Equals(other.disposalMethodNamePattern)) return false;
        if (handleTypeNameExclusionPattern.Equals(other.handleTypeNameExclusionPattern)) return false;


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


