using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SilkWrapped.SourceGenerator;
public static class SyntaxExtensions
{
    public static bool IsPartial(this TypeDeclarationSyntax typeDeclaration)
        => typeDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
}
