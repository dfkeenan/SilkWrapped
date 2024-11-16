using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SilkWrapped.ObjectModelTool.SyntaxTransformers;
internal class PointerToNullableType : CSharpSyntaxRewriter
{
    private readonly HashSet<string> parameterNames = new HashSet<string>();

    public override SyntaxNode? VisitVariableDeclaration(VariableDeclarationSyntax node)
    {
        PointerTypeSyntax? pointerType;
        if (IsNonVoidPointerSyntax(node.Type, out pointerType))
        {
            return node.WithType(NullableType(pointerType.ElementType).WithTriviaFrom(pointerType));
        }

        return base.VisitVariableDeclaration(node);
    }

    public override SyntaxNode? VisitParameterList(ParameterListSyntax node)
    {
        parameterNames.Clear();
        return base.VisitParameterList(node);
    }

    public override SyntaxNode? VisitParameter(ParameterSyntax node)
    {
        PointerTypeSyntax? pointerType;
        if (IsNonVoidPointerSyntax(node.Type, out pointerType))
        {
            parameterNames.Add(node.Identifier.Text);
            return node.WithType(NullableType(pointerType.ElementType).WithTriviaFrom(pointerType));
        }

        return base.VisitParameter(node);
    }

    private static bool IsNonVoidPointerSyntax(TypeSyntax? typeStyntax, [NotNullWhen(true)] out PointerTypeSyntax? pointerTypeSyntax)
    {
        pointerTypeSyntax = null;
        if (typeStyntax is PointerTypeSyntax pointerType && pointerType.ElementType is not PredefinedTypeSyntax { Keyword.Text: "void" })
        {
            pointerTypeSyntax = pointerType;
            return true;
        }

        return false;
    }

    public override SyntaxNode? VisitIfStatement(IfStatementSyntax node)
    {
        if (node.Condition 
            is BinaryExpressionSyntax 
            { 
                OperatorToken.Text: "!=",
                Left: IdentifierNameSyntax { Identifier.Text : string name }} && parameterNames.Contains(name))
        {
            node = node.WithCondition(ParseExpression($"{name}.HasValue"));
        }

        return base.VisitIfStatement(node);
    }

    public override SyntaxNode? VisitAssignmentExpression(AssignmentExpressionSyntax node)
    {
        if(node.Right is IdentifierNameSyntax { Identifier.Text: string name } && parameterNames.Contains(name))
        {
            return node.WithRight(ParseExpression($"{name}.Value"));
        }

        return base.VisitAssignmentExpression(node);
    }
}
