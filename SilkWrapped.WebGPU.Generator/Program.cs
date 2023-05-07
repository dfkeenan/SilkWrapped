using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Extensions.Disposal;
using System.IO;
using Microsoft.CodeAnalysis.MSBuild;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Build.Locator;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using Silk.NET.Core.Native;

var targetProjectPath = @"..\..\..\..\SilkWrapped.WebGPU\SilkWrapped.WebGPU.csproj";
var projectExists = File.Exists(targetProjectPath);

Console.WriteLine($"{targetProjectPath} {(projectExists ? "found" : "not found")}");

if (!projectExists) return;

var generatedPath = Path.Combine(Path.GetDirectoryName(targetProjectPath), "Generated");

if (Directory.Exists(generatedPath))
{
    new DirectoryInfo(generatedPath)
        .GetFiles()
        .ToList()
        .ForEach(f => f.Delete());
}

var webGPUtype = typeof(WebGPU);

var webGPUmethods = webGPUtype.GetMethods();

var typesToFilter = new[] { typeof(object), typeof(byte).MakeByRefType(), typeof(byte).MakePointerType() };
var voidPointerType = typeof(void).MakePointerType();


var typesToWrap = (from m in webGPUmethods
                   let p = m.GetParameters()
                   where p is { Length: > 0 } && !p.Any(x => typesToFilter.Contains(x.ParameterType))
                   group m by p[0].ParameterType into g
                   where g.Key != typeof(string)
                   where g.Key != typeof(string[])
                   where g.Key.Name.StartsWith("InstanceDescriptor") == false
                   select new
                   {
                       Type = g.Key,
                       Methods = g.ToList()
                   }).ToList();

typesToWrap.AddRange(from m in webGPUmethods
                     where m.ReturnType != typeof(void)
                     where m.ReturnType != voidPointerType
                     where m.ReturnType.IsPointer
                     where !typesToWrap.Any(t => t.Type == m.ReturnType)
                     group m by m.ReturnType into g
                     select new
                     {
                         Type = g.Key,
                         Methods = new List<MethodInfo>(),
                     });


string GetWrapperName(Type type)
{
    return $"{type.Name.Trim('*')}Wrapper";
}


var @namespace = (QualifiedNameSyntax)ParseName(Path.GetFileNameWithoutExtension(targetProjectPath))!;
var folders = new List<string>() { "Generated" };

foreach (var type in typesToWrap)
{
    var name = GetWrapperName(type.Type);
    var syntax = CreateWrapperSyntax(@namespace, name, type.Type, type.Methods);
    File.WriteAllText(Path.Combine(generatedPath, $"{name}.cs"), syntax.ToFullString());
}


CompilationUnitSyntax CreateWrapperSyntax(QualifiedNameSyntax @namespace, string wrapperName, Type type, IEnumerable<MethodInfo> methods)
{
    var pointerTypeName = type.Name.Trim('*');
    var isWeak = pointerTypeName != "Instance";

    return CompilationUnit()
.WithMembers(
    SingletonList<MemberDeclarationSyntax>(
        FileScopedNamespaceDeclaration(@namespace)
        .WithUsings(
            List<UsingDirectiveSyntax>(
                new UsingDirectiveSyntax[]{
                    UsingDirective(
                        QualifiedName(
                            QualifiedName(
                                QualifiedName(
                                    IdentifierName("Silk"),
                                    IdentifierName("NET")),
                                IdentifierName("WebGPU")),
                            IdentifierName("WebGPU")))
                    .WithAlias(
                        NameEquals(
                            IdentifierName("WebGPU"))),
                    UsingDirective(
                        QualifiedName(
                            QualifiedName(
                                QualifiedName(
                                    IdentifierName("Silk"),
                                    IdentifierName("NET")),
                                IdentifierName("WebGPU")),
                            IdentifierName(pointerTypeName)))
                    .WithAlias(
                        NameEquals(
                            IdentifierName(pointerTypeName)))}))
        .WithMembers(
            SingletonList<MemberDeclarationSyntax>(
                ClassDeclaration(wrapperName)
                .WithModifiers(
                    TokenList(
                        new[]{
                            Token(SyntaxKind.PublicKeyword),
                            Token(SyntaxKind.UnsafeKeyword),
                            Token(SyntaxKind.PartialKeyword)}))
                .WithMembers(GetMembers())))))
    .NormalizeWhitespace();


    BlockSyntax StrongConstructorBody()
    {
        return Block(
                                    ExpressionStatement(
                                        AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                ThisExpression(),
                                                IdentifierName("WebGPU")),
                                            BinaryExpression(
                                                SyntaxKind.CoalesceExpression,
                                                IdentifierName("webGPU"),
                                                ThrowExpression(
                                                    ObjectCreationExpression(
                                                        IdentifierName("ArgumentNullException"))
                                                    .WithArgumentList(
                                                        ArgumentList(
                                                            SingletonSeparatedList<ArgumentSyntax>(
                                                                Argument(
                                                                    InvocationExpression(
                                                                        IdentifierName(
                                                                            Identifier(
                                                                                TriviaList(),
                                                                                SyntaxKind.NameOfKeyword,
                                                                                "nameof",
                                                                                "nameof",
                                                                                TriviaList())))
                                                                    .WithArgumentList(
                                                                        ArgumentList(
                                                                            SingletonSeparatedList<ArgumentSyntax>(
                                                                                Argument(
                                                                                    IdentifierName("webGPU"))))))))))))),
                                    ExpressionStatement(
                                        AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                ThisExpression(),
                                                IdentifierName("Disposal")),
                                            BinaryExpression(
                                                SyntaxKind.CoalesceExpression,
                                                IdentifierName("disposal"),
                                                ThrowExpression(
                                                    ObjectCreationExpression(
                                                        IdentifierName("ArgumentNullException"))
                                                    .WithArgumentList(
                                                        ArgumentList(
                                                            SingletonSeparatedList<ArgumentSyntax>(
                                                                Argument(
                                                                    InvocationExpression(
                                                                        IdentifierName(
                                                                            Identifier(
                                                                                TriviaList(),
                                                                                SyntaxKind.NameOfKeyword,
                                                                                "nameof",
                                                                                "nameof",
                                                                                TriviaList())))
                                                                    .WithArgumentList(
                                                                        ArgumentList(
                                                                            SingletonSeparatedList<ArgumentSyntax>(
                                                                                Argument(
                                                                                    IdentifierName("disposal"))))))))))))),
                                    ExpressionStatement(
                                        AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                ThisExpression(),
                                                IdentifierName("RawPointer")),
                                            ConditionalExpression(
                                                BinaryExpression(
                                                    SyntaxKind.EqualsExpression,
                                                    IdentifierName("rawPointer"),
                                                    LiteralExpression(
                                                        SyntaxKind.NullLiteralExpression)),
                                                IdentifierName("rawPointer"),
                                                ThrowExpression(
                                                    ObjectCreationExpression(
                                                        IdentifierName("ArgumentNullException"))
                                                    .WithArgumentList(
                                                        ArgumentList(
                                                            SingletonSeparatedList<ArgumentSyntax>(
                                                                Argument(
                                                                    InvocationExpression(
                                                                        IdentifierName(
                                                                            Identifier(
                                                                                TriviaList(),
                                                                                SyntaxKind.NameOfKeyword,
                                                                                "nameof",
                                                                                "nameof",
                                                                                TriviaList())))
                                                                    .WithArgumentList(
                                                                        ArgumentList(
                                                                            SingletonSeparatedList<ArgumentSyntax>(
                                                                                Argument(
                                                                                    IdentifierName("rawPointer"))))))))))))));
    }

    BlockSyntax WeakConsturctorBody()
    {
        return Block(
                                    ExpressionStatement(
                                        AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            IdentifierName("weakWebGPUReference"),
                                            ObjectCreationExpression(
                                                GenericName(
                                                    Identifier("WeakReference"))
                                                .WithTypeArgumentList(
                                                    TypeArgumentList(
                                                        SingletonSeparatedList<TypeSyntax>(
                                                            IdentifierName("WebGPU")))))
                                            .WithArgumentList(
                                                ArgumentList(
                                                    SingletonSeparatedList<ArgumentSyntax>(
                                                        Argument(
                                                            BinaryExpression(
                                                                SyntaxKind.CoalesceExpression,
                                                                IdentifierName("webGPU"),
                                                                ThrowExpression(
                                                                    ObjectCreationExpression(
                                                                        IdentifierName("ArgumentNullException"))
                                                                    .WithArgumentList(
                                                                        ArgumentList(
                                                                            SingletonSeparatedList<ArgumentSyntax>(
                                                                                Argument(
                                                                                    InvocationExpression(
                                                                                        IdentifierName(
                                                                                            Identifier(
                                                                                                TriviaList(),
                                                                                                SyntaxKind.NameOfKeyword,
                                                                                                "nameof",
                                                                                                "nameof",
                                                                                                TriviaList())))
                                                                                    .WithArgumentList(
                                                                                        ArgumentList(
                                                                                            SingletonSeparatedList<ArgumentSyntax>(
                                                                                                Argument(
                                                                                                    IdentifierName("webGPU"))))))))))))))))),
                                    ExpressionStatement(
                                        AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            IdentifierName("weakWebGPUDisposalReference"),
                                            ObjectCreationExpression(
                                                GenericName(
                                                    Identifier("WeakReference"))
                                                .WithTypeArgumentList(
                                                    TypeArgumentList(
                                                        SingletonSeparatedList<TypeSyntax>(
                                                            IdentifierName("WebGPUDisposal")))))
                                            .WithArgumentList(
                                                ArgumentList(
                                                    SingletonSeparatedList<ArgumentSyntax>(
                                                        Argument(
                                                            BinaryExpression(
                                                                SyntaxKind.CoalesceExpression,
                                                                IdentifierName("disposal"),
                                                                ThrowExpression(
                                                                    ObjectCreationExpression(
                                                                        IdentifierName("ArgumentNullException"))
                                                                    .WithArgumentList(
                                                                        ArgumentList(
                                                                            SingletonSeparatedList<ArgumentSyntax>(
                                                                                Argument(
                                                                                    InvocationExpression(
                                                                                        IdentifierName(
                                                                                            Identifier(
                                                                                                TriviaList(),
                                                                                                SyntaxKind.NameOfKeyword,
                                                                                                "nameof",
                                                                                                "nameof",
                                                                                                TriviaList())))
                                                                                    .WithArgumentList(
                                                                                        ArgumentList(
                                                                                            SingletonSeparatedList<ArgumentSyntax>(
                                                                                                Argument(
                                                                                                    IdentifierName("disposal"))))))))))))))))),
                                    ExpressionStatement(
                                        AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                ThisExpression(),
                                                IdentifierName("RawPointer")),
                                            ConditionalExpression(
                                                BinaryExpression(
                                                    SyntaxKind.EqualsExpression,
                                                    IdentifierName("rawPointer"),
                                                    LiteralExpression(
                                                        SyntaxKind.NullLiteralExpression)),
                                                IdentifierName("rawPointer"),
                                                ThrowExpression(
                                                    ObjectCreationExpression(
                                                        IdentifierName("ArgumentNullException"))
                                                    .WithArgumentList(
                                                        ArgumentList(
                                                            SingletonSeparatedList<ArgumentSyntax>(
                                                                Argument(
                                                                    InvocationExpression(
                                                                        IdentifierName(
                                                                            Identifier(
                                                                                TriviaList(),
                                                                                SyntaxKind.NameOfKeyword,
                                                                                "nameof",
                                                                                "nameof",
                                                                                TriviaList())))
                                                                    .WithArgumentList(
                                                                        ArgumentList(
                                                                            SingletonSeparatedList<ArgumentSyntax>(
                                                                                Argument(
                                                                                    IdentifierName("rawPointer"))))))))))))));
    }

    List<MemberDeclarationSyntax> StrongPropertyMembers()
    {
        return new List<MemberDeclarationSyntax>
        {
            PropertyDeclaration(
                                IdentifierName("WebGPU"),
                                Identifier("WebGPU"))
                            .WithAttributeLists(
                                SingletonList<AttributeListSyntax>(
                                    AttributeList(
                                        SeparatedList<AttributeSyntax>(
                                            new SyntaxNodeOrToken[]{
                                                Attribute(
                                                    IdentifierName("EditorBrowsable"))
                                                .WithArgumentList(
                                                    AttributeArgumentList(
                                                        SingletonSeparatedList<AttributeArgumentSyntax>(
                                                            AttributeArgument(
                                                                MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    IdentifierName("EditorBrowsableState"),
                                                                    IdentifierName("Advanced")))))),
                                                Token(SyntaxKind.CommaToken),
                                                Attribute(
                                                    IdentifierName("Browsable"))
                                                .WithArgumentList(
                                                    AttributeArgumentList(
                                                        SingletonSeparatedList<AttributeArgumentSyntax>(
                                                            AttributeArgument(
                                                                LiteralExpression(
                                                                    SyntaxKind.FalseLiteralExpression)))))}))))
                            .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.PublicKeyword)))
                            .WithAccessorList(
                                AccessorList(
                                    SingletonList<AccessorDeclarationSyntax>(
                                        AccessorDeclaration(
                                            SyntaxKind.GetAccessorDeclaration)
                                        .WithSemicolonToken(
                                            Token(SyntaxKind.SemicolonToken))))),
                            PropertyDeclaration(
                                IdentifierName("WebGPUDisposal"),
                                Identifier("Disposal"))
                            .WithAttributeLists(
                                SingletonList<AttributeListSyntax>(
                                    AttributeList(
                                        SeparatedList<AttributeSyntax>(
                                            new SyntaxNodeOrToken[]{
                                                Attribute(
                                                    IdentifierName("EditorBrowsable"))
                                                .WithArgumentList(
                                                    AttributeArgumentList(
                                                        SingletonSeparatedList<AttributeArgumentSyntax>(
                                                            AttributeArgument(
                                                                MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    IdentifierName("EditorBrowsableState"),
                                                                    IdentifierName("Advanced")))))),
                                                Token(SyntaxKind.CommaToken),
                                                Attribute(
                                                    IdentifierName("Browsable"))
                                                .WithArgumentList(
                                                    AttributeArgumentList(
                                                        SingletonSeparatedList<AttributeArgumentSyntax>(
                                                            AttributeArgument(
                                                                LiteralExpression(
                                                                    SyntaxKind.FalseLiteralExpression)))))}))))
                            .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.PublicKeyword)))
                            .WithAccessorList(
                                AccessorList(
                                    SingletonList<AccessorDeclarationSyntax>(
                                        AccessorDeclaration(
                                            SyntaxKind.GetAccessorDeclaration)
                                        .WithSemicolonToken(
                                            Token(SyntaxKind.SemicolonToken))))),
                            PropertyDeclaration(
                                PointerType(
                                    IdentifierName(pointerTypeName)),
                                Identifier("RawPointer"))
                            .WithAttributeLists(
                                SingletonList<AttributeListSyntax>(
                                    AttributeList(
                                        SeparatedList<AttributeSyntax>(
                                            new SyntaxNodeOrToken[]{
                                                Attribute(
                                                    IdentifierName("EditorBrowsable"))
                                                .WithArgumentList(
                                                    AttributeArgumentList(
                                                        SingletonSeparatedList<AttributeArgumentSyntax>(
                                                            AttributeArgument(
                                                                MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    IdentifierName("EditorBrowsableState"),
                                                                    IdentifierName("Advanced")))))),
                                                Token(SyntaxKind.CommaToken),
                                                Attribute(
                                                    IdentifierName("Browsable"))
                                                .WithArgumentList(
                                                    AttributeArgumentList(
                                                        SingletonSeparatedList<AttributeArgumentSyntax>(
                                                            AttributeArgument(
                                                                LiteralExpression(
                                                                    SyntaxKind.FalseLiteralExpression)))))}))))
                            .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.PublicKeyword)))
                            .WithAccessorList(
                                AccessorList(
                                    SingletonList<AccessorDeclarationSyntax>(
                                        AccessorDeclaration(
                                            SyntaxKind.GetAccessorDeclaration)
                                        .WithSemicolonToken(
                                            Token(SyntaxKind.SemicolonToken)))))
        };
    }

    List<MemberDeclarationSyntax> WeakPropertyMembers()
    {
        return new List<MemberDeclarationSyntax>
        {
            FieldDeclaration(
                                VariableDeclaration(
                                    GenericName(
                                        Identifier("WeakReference"))
                                    .WithTypeArgumentList(
                                        TypeArgumentList(
                                            SingletonSeparatedList<TypeSyntax>(
                                                IdentifierName("WebGPU")))))
                                .WithVariables(
                                    SingletonSeparatedList<VariableDeclaratorSyntax>(
                                        VariableDeclarator(
                                            Identifier("weakWebGPUReference"))))),
                            PropertyDeclaration(
                                NullableType(
                                    IdentifierName("WebGPU")),
                                Identifier("WebGPU"))
                            .WithAttributeLists(
                                SingletonList<AttributeListSyntax>(
                                    AttributeList(
                                        SeparatedList<AttributeSyntax>(
                                            new SyntaxNodeOrToken[]{
                                                Attribute(
                                                    IdentifierName("EditorBrowsable"))
                                                .WithArgumentList(
                                                    AttributeArgumentList(
                                                        SingletonSeparatedList<AttributeArgumentSyntax>(
                                                            AttributeArgument(
                                                                MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    IdentifierName("EditorBrowsableState"),
                                                                    IdentifierName("Advanced")))))),
                                                Token(SyntaxKind.CommaToken),
                                                Attribute(
                                                    IdentifierName("Browsable"))
                                                .WithArgumentList(
                                                    AttributeArgumentList(
                                                        SingletonSeparatedList<AttributeArgumentSyntax>(
                                                            AttributeArgument(
                                                                LiteralExpression(
                                                                    SyntaxKind.FalseLiteralExpression)))))}))))
                            .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.PublicKeyword)))
                            .WithExpressionBody(
                                ArrowExpressionClause(
                                    ConditionalExpression(
                                        InvocationExpression(
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                IdentifierName("weakWebGPUReference"),
                                                IdentifierName("TryGetTarget")))
                                        .WithArgumentList(
                                            ArgumentList(
                                                SingletonSeparatedList<ArgumentSyntax>(
                                                    Argument(
                                                        DeclarationExpression(
                                                            IdentifierName(
                                                                Identifier(
                                                                    TriviaList(),
                                                                    SyntaxKind.VarKeyword,
                                                                    "var",
                                                                    "var",
                                                                    TriviaList())),
                                                            SingleVariableDesignation(
                                                                Identifier("target"))))
                                                    .WithRefOrOutKeyword(
                                                        Token(SyntaxKind.OutKeyword))))),
                                        IdentifierName("target"),
                                        LiteralExpression(
                                            SyntaxKind.NullLiteralExpression))))
                            .WithSemicolonToken(
                                Token(SyntaxKind.SemicolonToken)),
                            FieldDeclaration(
                                VariableDeclaration(
                                    GenericName(
                                        Identifier("WeakReference"))
                                    .WithTypeArgumentList(
                                        TypeArgumentList(
                                            SingletonSeparatedList<TypeSyntax>(
                                                IdentifierName("WebGPUDisposal")))))
                                .WithVariables(
                                    SingletonSeparatedList<VariableDeclaratorSyntax>(
                                        VariableDeclarator(
                                            Identifier("weakWebGPUDisposalReference"))))),
                            PropertyDeclaration(
                                NullableType(
                                    IdentifierName("WebGPUDisposal")),
                                Identifier("Disposal"))
                            .WithAttributeLists(
                                SingletonList<AttributeListSyntax>(
                                    AttributeList(
                                        SeparatedList<AttributeSyntax>(
                                            new SyntaxNodeOrToken[]{
                                                Attribute(
                                                    IdentifierName("EditorBrowsable"))
                                                .WithArgumentList(
                                                    AttributeArgumentList(
                                                        SingletonSeparatedList<AttributeArgumentSyntax>(
                                                            AttributeArgument(
                                                                MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    IdentifierName("EditorBrowsableState"),
                                                                    IdentifierName("Advanced")))))),
                                                Token(SyntaxKind.CommaToken),
                                                Attribute(
                                                    IdentifierName("Browsable"))
                                                .WithArgumentList(
                                                    AttributeArgumentList(
                                                        SingletonSeparatedList<AttributeArgumentSyntax>(
                                                            AttributeArgument(
                                                                LiteralExpression(
                                                                    SyntaxKind.FalseLiteralExpression)))))}))))
                            .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.PublicKeyword)))
                            .WithExpressionBody(
                                ArrowExpressionClause(
                                    ConditionalExpression(
                                        InvocationExpression(
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                IdentifierName("weakWebGPUDisposalReference"),
                                                IdentifierName("TryGetTarget")))
                                        .WithArgumentList(
                                            ArgumentList(
                                                SingletonSeparatedList<ArgumentSyntax>(
                                                    Argument(
                                                        DeclarationExpression(
                                                            IdentifierName(
                                                                Identifier(
                                                                    TriviaList(),
                                                                    SyntaxKind.VarKeyword,
                                                                    "var",
                                                                    "var",
                                                                    TriviaList())),
                                                            SingleVariableDesignation(
                                                                Identifier("target"))))
                                                    .WithRefOrOutKeyword(
                                                        Token(SyntaxKind.OutKeyword))))),
                                        IdentifierName("target"),
                                        LiteralExpression(
                                            SyntaxKind.NullLiteralExpression))))
                            .WithSemicolonToken(
                                Token(SyntaxKind.SemicolonToken)),
                            PropertyDeclaration(
                                PointerType(
                                    IdentifierName(pointerTypeName!)),
                                Identifier("RawPointer"))
                            .WithAttributeLists(
                                SingletonList<AttributeListSyntax>(
                                    AttributeList(
                                        SeparatedList<AttributeSyntax>(
                                            new SyntaxNodeOrToken[]{
                                                Attribute(
                                                    IdentifierName("EditorBrowsable"))
                                                .WithArgumentList(
                                                    AttributeArgumentList(
                                                        SingletonSeparatedList<AttributeArgumentSyntax>(
                                                            AttributeArgument(
                                                                MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    IdentifierName("EditorBrowsableState"),
                                                                    IdentifierName("Advanced")))))),
                                                Token(SyntaxKind.CommaToken),
                                                Attribute(
                                                    IdentifierName("Browsable"))
                                                .WithArgumentList(
                                                    AttributeArgumentList(
                                                        SingletonSeparatedList<AttributeArgumentSyntax>(
                                                            AttributeArgument(
                                                                LiteralExpression(
                                                                    SyntaxKind.FalseLiteralExpression)))))}))))
                            .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.PublicKeyword)))
                            .WithAccessorList(
                                AccessorList(
                                    SingletonList<AccessorDeclarationSyntax>(
                                        AccessorDeclaration(
                                            SyntaxKind.GetAccessorDeclaration)
                                        .WithSemicolonToken(
                                            Token(SyntaxKind.SemicolonToken)))))
        };
    }

    SyntaxList<MemberDeclarationSyntax> GetMembers()
    {
        var result = new List<MemberDeclarationSyntax>()
        {
             ConstructorDeclaration(
                        Identifier(wrapperName))
                    .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.PublicKeyword)))
                    .WithParameterList(
                        ParameterList(
                            SeparatedList<ParameterSyntax>(
                                new SyntaxNodeOrToken[]{
                                    Parameter(
                                        Identifier("webGPU"))
                                    .WithType(
                                        IdentifierName("WebGPU")),
                                    Token(SyntaxKind.CommaToken),
                                    Parameter(
                                        Identifier("disposal"))
                                    .WithType(
                                        IdentifierName("WebGPUDisposal")),
                                    Token(SyntaxKind.CommaToken),
                                    Parameter(
                                        Identifier("rawPointer"))
                                    .WithType(
                                        PointerType(
                                            IdentifierName(pointerTypeName)))})))
                    .WithBody(isWeak ? WeakConsturctorBody() : StrongConstructorBody())
        };

        result.AddRange(isWeak ? WeakPropertyMembers() : StrongPropertyMembers());

        result.AddRange(GetMethods());


        result.Add(ConversionOperatorDeclaration(
                                Token(SyntaxKind.ImplicitKeyword),
                                PointerType(
                                    IdentifierName(pointerTypeName)))
                            .WithModifiers(
                                TokenList(
                                    new[]{
                                        Token(SyntaxKind.PublicKeyword),
                                        Token(SyntaxKind.StaticKeyword)}))
                            .WithParameterList(
                                ParameterList(
                                    SingletonSeparatedList<ParameterSyntax>(
                                        Parameter(
                                            Identifier("wrapper"))
                                        .WithType(
                                            IdentifierName(wrapperName)))))
                            .WithExpressionBody(
                                ArrowExpressionClause(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("wrapper"),
                                        IdentifierName("RawPointer"))))
                            .WithSemicolonToken(
                                Token(SyntaxKind.SemicolonToken)));
        return new SyntaxList<MemberDeclarationSyntax>(result);
    }

    IEnumerable<MemberDeclarationSyntax> GetMethods()
    {
        foreach (var method in methods)
        {
            var parameters = method.GetParameters();

            var secondParameter = parameters.Skip(1).FirstOrDefault();

            //Dodgy filter to remove pointer based overloads;
            if(secondParameter is { ParameterType.IsPointer: true })
            {
                continue;
            }

            if(parameters.Any(p => p.ParameterType == voidPointerType))
            {
                continue;
            }


            var methodName = method.Name.Replace(pointerTypeName!, "");

            var parameterSyntaxes = (from p in parameters.Skip(1) //assuming the first is the raw pointer
                                     let parameterSyntax = Parameter(Identifier(p.Name!.Trim('&'))).WithType(SyntaxFactory.ParseTypeName(p.ParameterType.Name.Trim('&')))
                                     select p switch
                                     {
                                         { IsIn: true } => parameterSyntax.WithModifiers(TokenList(Token(SyntaxKind.InKeyword))),
                                         { IsOut: true } => parameterSyntax.WithModifiers(TokenList(Token(SyntaxKind.OutKeyword))),
                                         { ParameterType.IsByRef: true } => parameterSyntax.WithModifiers(TokenList(Token(SyntaxKind.RefKeyword))),
                                         _ => parameterSyntax
                                     }).ToList();
                                    

            var parametersSeparatedList = SeparatedList<ParameterSyntax>(parameterSyntaxes);


            var argumentsSyntaxes = new List<SyntaxNodeOrToken>();

            for (int i = 0; i < parameters.Length; i++)
            {
                if(i == 0)
                {
                    //assuming the first is the raw pointer
                    argumentsSyntaxes.Add(Argument(IdentifierName("RawPointer")));
                    continue;
                }

                argumentsSyntaxes.Add(Token(SyntaxKind.CommaToken));

                var argument = Argument(IdentifierName(parameters[i].Name!.Trim('&')));


                argument = parameters[i] switch
                {
                    { IsIn: true } => argument,
                    { IsOut: true } => argument.WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword)),
                    { ParameterType.IsByRef: true } => argument.WithRefOrOutKeyword(Token(SyntaxKind.RefKeyword)),
                    _ => argument
                };

                argumentsSyntaxes.Add(argument);
            }

            var argumentsSeparatedList = SeparatedList<ArgumentSyntax>(argumentsSyntaxes.ToArray());


            TypeSyntax returnType;
            BlockSyntax methodCall = Block();

            if(method.ReturnType == typeof(void))
            {
                returnType = PredefinedType(Token(SyntaxKind.VoidKeyword));

                methodCall = Block(SingletonList<StatementSyntax>(
                                                    ExpressionStatement(
                                                        InvocationExpression(
                                                            MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                IdentifierName("webGPU"),
                                                                IdentifierName(method.Name)))
                                                        .WithArgumentList(
                                                            ArgumentList(
                                                                argumentsSeparatedList)))));
            }
            else if(method.ReturnType == voidPointerType)
            {
                returnType = ParseTypeName("ReadOnlySpan<byte>");
                methodCall = Block(SingletonList<StatementSyntax>(
                                   ReturnStatement(
                                       ObjectCreationExpression(returnType)
                                        .WithArgumentList(
                                            ArgumentList(
                                                SeparatedList<ArgumentSyntax>(
                                                    new SyntaxNodeOrToken[]{
                                                        Argument(
                                                            InvocationExpression(
                                                            MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                IdentifierName("webGPU"),
                                                                IdentifierName(method.Name)))
                                                        .WithArgumentList(
                                                            ArgumentList(
                                                                argumentsSeparatedList))),
                                                        Token(SyntaxKind.CommaToken),
                                                        Argument(CastExpression(IdentifierName("int"),IdentifierName("size")))})
                                                )))));

            }
            else if(method.ReturnType.IsPointer) 
            {
               returnType = ParseTypeName(GetWrapperName(method.ReturnType));
                methodCall = Block(SingletonList<StatementSyntax>(
                                    ReturnStatement(
                                        ObjectCreationExpression(returnType)
                                        .WithArgumentList(
                                            ArgumentList(
                                                SeparatedList<ArgumentSyntax>(
                                                    new SyntaxNodeOrToken[]{
                                                        Argument(
                                                            IdentifierName("WebGPU")),
                                                        Token(SyntaxKind.CommaToken),
                                                        Argument(
                                                            IdentifierName("Disposal")),
                                                        Token(SyntaxKind.CommaToken),
                                                        Argument(
                                                            InvocationExpression(
                                                            MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                IdentifierName("webGPU"),
                                                                IdentifierName(method.Name)))
                                                        .WithArgumentList(
                                                            ArgumentList(
                                                                argumentsSeparatedList)))}))))));
            }
            else
            {
                returnType = ParseTypeName(method.ReturnType.Name);

                methodCall = Block(SingletonList<StatementSyntax>(
                                    ReturnStatement(
                                        InvocationExpression(
                                                MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    IdentifierName("webGPU"),
                                                    IdentifierName(method.Name)))
                                            .WithArgumentList(
                                                ArgumentList(
                                                    argumentsSeparatedList)))));
            }

            TypeParameterListSyntax genericTypeParameters = null;
            SyntaxList<TypeParameterConstraintClauseSyntax> genericConstraints = default;

            if (method.IsGenericMethod)
            {
                var genericArgs = method.GetGenericMethodDefinition().GetGenericArguments();
                var typeParams = genericArgs.Select(a => TypeParameter(Identifier(a.Name))).ToList();
                genericTypeParameters = TypeParameterList(SeparatedList<TypeParameterSyntax>(typeParams));
                //TODO: Generic constraints properly.
                var constraints = genericArgs.Select(a => TypeParameterConstraintClause(
                                        IdentifierName(a.Name))
                                    .WithConstraints(
                                        SeparatedList<TypeParameterConstraintSyntax>(
                                            from c in a.GetGenericParameterConstraints()
                                            let identifierName = c switch
                                            {
                                                Type t when t == typeof(NativeExtension<WebGPU>) => IdentifierName("NativeExtension<WebGPU>"),
                                                _ => IdentifierName(
                                                    Identifier(
                                                        TriviaList(),
                                                        SyntaxKind.UnmanagedKeyword,
                                                        "unmanaged",
                                                        "unmanaged",
                                                        TriviaList()))
                                            }
                                            select TypeConstraint(identifierName)
                                            ))).ToList();

                if(constraints.Any())
                {
                    genericConstraints = new SyntaxList<TypeParameterConstraintClauseSyntax>(constraints);
                }
            }

            yield return MethodDeclaration(returnType,
                                Identifier(methodName))
                            .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.PublicKeyword)))
                            .WithTypeParameterList(genericTypeParameters)
                            .WithParameterList(
                                ParameterList(parametersSeparatedList))
                                .WithConstraintClauses(genericConstraints)
                            .WithBody(
                                Block(
                                    SingletonList<StatementSyntax>(
                                        IfStatement(
                                            IsPatternExpression(
                                                IdentifierName("WebGPU"),
                                                DeclarationPattern(
                                                    IdentifierName("WebGPU"),
                                                    SingleVariableDesignation(
                                                        Identifier("webGPU")))),
                                            methodCall)
                                        .WithElse(
                                            ElseClause(
                                                Block(
                                                    SingletonList<StatementSyntax>(
                                                        ThrowStatement(
                                                            ObjectCreationExpression(
                                                                IdentifierName("InvalidOperationException"))
                                                            .WithArgumentList(
                                                                ArgumentList(
                                                                    SingletonSeparatedList<ArgumentSyntax>(
                                                                        Argument(
                                                                            LiteralExpression(
                                                                                SyntaxKind.StringLiteralExpression,
                                                                                Literal("WebGPU is null"))))))))))))));
        }
    }
}
