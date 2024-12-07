using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilkWrapped.ObjectModelTool.Rewriters;
internal class ImplementFunctionPointer : ContextAwareCSharpSyntaxRewriter
{
    public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node)
    {
        var toRemove = new List<SyntaxNode>();

        foreach (var item in node.Members)
        {
            switch (item)
            {
                case FieldDeclarationSyntax fieldDeclarationSyntax:
                    toRemove.Add(fieldDeclarationSyntax);
                    break;
                case PropertyDeclarationSyntax propertyDeclarationSyntax: 
                    toRemove.Add(propertyDeclarationSyntax); 
                    break;
                case ConversionOperatorDeclarationSyntax operatorDeclarationSyntax:
                    toRemove.Add(operatorDeclarationSyntax);
                    break;
                case ConstructorDeclarationSyntax {ParameterList.Parameters: [ParameterSyntax{Type: FunctionPointerTypeSyntax } ,..] } constructorDeclarationSyntax:
                    toRemove.Add(constructorDeclarationSyntax);
                    break;
                default:
                    break;
            }
        }

        node = node.RemoveNodes(toRemove, SyntaxRemoveOptions.KeepNoTrivia)!;

        var members = node.Members;

        var apiType = ParseTypeName($"{Context.ApiTypeSymbol.ContainingNamespace.ToDisplayString()}.{node.Identifier.Text}");

        members = members.Insert(0, ParseMemberDeclaration($"private readonly {apiType} callback;")!);
        members = members.Add(ParseMemberDeclaration($"public static implicit operator {apiType}({node.Identifier.Text} callback) => callback.callback;")!);


        node = node.WithMembers(members);


        return base.VisitStructDeclaration(node);
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if(node.Identifier.Text == nameof(IDisposable.Dispose))
        {
            node = node.WithBody(null)
                       .WithExpressionBody(ArrowExpressionClause(ParseExpression("callback.Dispose()")))
                       .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        return base.VisitMethodDeclaration(node);
    }

    public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {

        if (TypeName(node.ParameterList.Parameters[0].Type) is string typeName &&
            Context.TryGetApiTypeSymbol(typeName, out var apiType) &&
            apiType.DelegateInvokeMethod is IMethodSymbol delegateType)
        {
            string parameters = string.Join(", ", delegateType.Parameters.Select(p => p.Name));

            var arguments = from parameter in delegateType.Parameters
                            let type = parameter.Type
                            select parameter switch
                            {
                                {Type.TypeKind: TypeKind.Enum } => $"({type.Name}){parameter.Name}",
                                {Type: IPointerTypeSymbol {PointedAtType.SpecialType: SpecialType.System_Byte} } 
                                    => $"SilkMarshal.PtrToString((nint){parameter.Name}, NativeStringEncoding.UTF8)",
                                _ => parameter.Name
                            };

       var statement = $$"""
                                callback = new(({{parameters}}) =>
                                {
                                    proc({{string.Join(", ", arguments)}});
                                });
                              """;

            node = node.WithBody(Block(ParseStatement(statement)));


        }

        

        return base.VisitConstructorDeclaration(node);
    }
}
