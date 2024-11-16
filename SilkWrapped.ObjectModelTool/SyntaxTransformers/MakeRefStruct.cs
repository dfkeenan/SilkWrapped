using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SilkWrapped.ObjectModelTool.SyntaxTransformers;
internal class MakeRefStruct : CSharpSyntaxRewriter
{
    private static readonly SyntaxToken refToken = ParseToken("ref ");
    public static MakeRefStruct Instance { get; } = new MakeRefStruct();
    public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node)
    {
        
        if (node.Modifiers.IndexOf(SyntaxKind.PartialKeyword) is int index and >= 0 )
        {
            node = node.WithModifiers(node.Modifiers.Insert(index, refToken));
        }
        else
        {
            node = node.AddModifiers(refToken);
        }

        return base.VisitStructDeclaration(node);
    }
}
