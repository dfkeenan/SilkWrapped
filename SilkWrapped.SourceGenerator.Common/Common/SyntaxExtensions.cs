using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SilkWrapped.SourceGenerator.Common;
public static class SyntaxExtensions
{
    public static bool IsPartial(this TypeDeclarationSyntax typeDeclaration)
        => typeDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));

    public static bool IsVoid(this TypeSyntax type)
        => type is PredefinedTypeSyntax { Keyword.Text: "void" };
}
