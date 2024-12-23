using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SilkWrapped.SourceGenerator.Common;
public static class SyntaxExtensions
{
    public static bool IsPartial<T>(this T declaration)
        where T : MemberDeclarationSyntax
    {
        foreach (var modifier in declaration.Modifiers)
        {
            if (modifier.IsKind(SyntaxKind.PartialKeyword)) return true;
        }

        return false;
    }

    public static bool IsVoid(this TypeSyntax type)
        => type is PredefinedTypeSyntax { Keyword.Text: "void" };


    public static string GetDeclaration<T>(this T declaration)
        where T : MemberDeclarationSyntax
    {
        var start = declaration.Modifiers.Span.Start;
        var end = declaration switch
        {
            TypeDeclarationSyntax type => type.Identifier.Span.End,
            PropertyDeclarationSyntax property => property.Identifier.Span.End,
            MethodDeclarationSyntax method => method.ParameterList.Span.End,
            _ => start
        };
        start -= declaration.FullSpan.Start;
        end -= declaration.FullSpan.Start;

        var length = end - start;

        var result = declaration.GetText().GetSubText(new TextSpan(start, length)).ToString();

        return result;
    }
}
