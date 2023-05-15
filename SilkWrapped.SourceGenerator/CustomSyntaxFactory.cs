using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SilkWrapped.SourceGenerator;
internal static class CustomSyntaxFactory
{
    public static ClassDeclarationSyntax ClassDeclaration(string identifier, params SyntaxKind[] modifiers)
        => SyntaxFactory.ClassDeclaration(identifier)
                .WithModifiers(modifiers);

    public static ClassDeclarationSyntax AddConstructor(this ClassDeclarationSyntax classDeclaration, params PropertyDeclarationSyntax[] properties)
    {
        var parameters = ParameterList(new SeparatedSyntaxList<ParameterSyntax>().AddRange(properties.Select(p => Parameter(CamelCase(p.Identifier)).WithType(p.Type))));
        var assignments = properties.Select(p => ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(p.Identifier.Text),
                    IdentifierName(CamelCase(p.Identifier.Text)))));

        var constructor = ConstructorDeclaration(classDeclaration.Identifier)
                            .WithModifiers(SyntaxKind.PublicKeyword)
                            .WithParameterList(parameters)
                            .WithBody(Block(assignments));

        return classDeclaration.AddMembers(constructor);
    }

    public static ClassDeclarationSyntax AddDefaultConstructor(this ClassDeclarationSyntax classDeclaration, Func<BlockSyntax,BlockSyntax> blockBuilder)
    {
        var constructor = ConstructorDeclaration(classDeclaration.Identifier)
                            .WithModifiers(SyntaxKind.PublicKeyword)
                            .WithBody(blockBuilder(Block()));

        return classDeclaration.AddMembers(constructor);
    }

    public static BlockSyntax AddAssignmentStatement(this BlockSyntax block, string propertyName, ExpressionSyntax expression)
    {
        return block.AddStatements(AssignmentStatement(propertyName, expression));
    }

    public static ExpressionStatementSyntax AssignmentStatement(string propertyName, ExpressionSyntax expression)
    {
        return ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName(propertyName),
                            expression));
    }

    public static BlockSyntax AddStatements(this BlockSyntax block, IEnumerable<StatementSyntax> items)
        => block.WithStatements(block.Statements.AddRange(items));


    public static PropertyDeclarationSyntax PropertyDeclaration(ITypeSymbol typeSymbol, string identifier, params SyntaxKind[] modifiers)
        => SyntaxFactory.PropertyDeclaration(TypeSyntax(typeSymbol),identifier)
                .WithModifiers(modifiers)
                .AddAccessorListAccessors(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

    public static PropertyDeclarationSyntax PropertyDeclaration(string typeNameIdentifier, string identifier, params SyntaxKind[] modifiers)
        => SyntaxFactory.PropertyDeclaration(IdentifierName(typeNameIdentifier), identifier)
                .WithModifiers(modifiers)
                .AddAccessorListAccessors(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

    public static PropertyDeclarationSyntax WithSetter(this PropertyDeclarationSyntax propertyDeclaration, params SyntaxKind[] modifiers)
        => propertyDeclaration.AddAccessorListAccessors(AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken)).WithModifiers(new SyntaxTokenList(modifiers.Select(m => Token(m)))));

    public static ClassDeclarationSyntax AddDispose(this ClassDeclarationSyntax classDeclaration, Func<BlockSyntax, BlockSyntax> blockBuilder)
    {
        var dispose = MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("Dispose"))
                            .WithModifiers(SyntaxKind.PublicKeyword)
                            .WithBody(blockBuilder(Block()));

        return classDeclaration.AddBaseListTypes(Disposable).AddMembers(dispose);
    }

    private static readonly SimpleBaseTypeSyntax Disposable = SimpleBaseType(
                        QualifiedName(
                            IdentifierName("System"),
                            IdentifierName("IDisposable")));

    public static ExpressionStatementSyntax InvokeDispose(ExpressionSyntax expression, bool conditional = false)
        =>  conditional ?
            ExpressionStatement(
                    ConditionalAccessExpression(
                        expression,
                        InvocationExpression(
                            MemberBindingExpression(
                                IdentifierName("Dispose"))))) :
            ExpressionStatement(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            expression,
                            IdentifierName("Dispose"))));

    public static ExpressionStatementSyntax InvokeDispose(string identifier, bool conditional = false)
        => InvokeDispose(IdentifierName(identifier), conditional);

    public static T WithModifiers<T>(this T memberDeclaration, params SyntaxKind[] modifiers) where T : MemberDeclarationSyntax
        => (T)memberDeclaration.WithModifiers(new SyntaxTokenList(modifiers.Select(m => Token(m))));

    public static T AddMembers<T>(this T typeDeclaration, IEnumerable<MemberDeclarationSyntax> items) where T : TypeDeclarationSyntax
        => (T)typeDeclaration.WithMembers( typeDeclaration.Members.AddRange(items));

    public static TypeSyntax TypeSyntax(ITypeSymbol typeSymbol)
        => ParseTypeName(typeSymbol.ToDisplayString());

    public static string CamelCase(string value)
        => char.ToLower(value[0]) + value.Substring(1);

    public static SyntaxToken CamelCase(SyntaxToken value)
        => Identifier(CamelCase(value.Text));
}
