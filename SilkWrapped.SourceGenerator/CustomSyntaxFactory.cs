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
                .WithModifiers(new SyntaxTokenList(modifiers.Select(m => Token(m))));

    public static PropertyDeclarationSyntax PropertyDeclaration(ITypeSymbol typeSymbol, string identifier, params SyntaxKind[] modifiers)
        => SyntaxFactory.PropertyDeclaration(TypeSyntax(typeSymbol),identifier)
                .WithModifiers(new SyntaxTokenList(modifiers.Select(m => Token(m))));

    public static TypeSyntax TypeSyntax(ITypeSymbol typeSymbol)
        => ParseTypeName(typeSymbol.ToDisplayString());
}
