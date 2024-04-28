using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
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
        var parameters = ParameterList(new SeparatedSyntaxList<ParameterSyntax>().AddRange(properties.Select(p => SyntaxFactory.Parameter(CamelCase(p.Identifier)).WithType(p.Type))));
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

    public static ArgumentSyntax Argument(IParameterSymbol parameterSymbol, bool ignoreRefKind = false)
    {
        ArgumentSyntax result = SyntaxFactory.Argument(IdentifierName(parameterSymbol.Name));

        if(ignoreRefKind) return result;

        return parameterSymbol.RefKind switch
        {
            RefKind.Ref => result.WithRefOrOutKeyword(Token(SyntaxKind.RefKeyword)),
            RefKind.Out => result.WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword)),
            _ => result
        };
    }

    public static T WithParameterList<T>(this T declaration, IEnumerable<ParameterSyntax> parameters) where T : BaseMethodDeclarationSyntax
        => (T)declaration.WithParameterList(ParameterList(new SeparatedSyntaxList<ParameterSyntax>().AddRange(parameters)));

    public static DelegateDeclarationSyntax WithParameterList(this DelegateDeclarationSyntax declaration, IEnumerable<ParameterSyntax> parameters)
        => declaration.WithParameterList(ParameterList(new SeparatedSyntaxList<ParameterSyntax>().AddRange(parameters)));
    public static ParenthesizedLambdaExpressionSyntax WithParameterList(this ParenthesizedLambdaExpressionSyntax declaration, IEnumerable<ParameterSyntax> parameters)
        => declaration.WithParameterList(ParameterList(new SeparatedSyntaxList<ParameterSyntax>().AddRange(parameters)));

    public static InvocationExpressionSyntax WithArgumentList(this InvocationExpressionSyntax declaration, IEnumerable<ArgumentSyntax> arguments)
        => declaration.WithArgumentList(ArgumentList(new SeparatedSyntaxList<ArgumentSyntax>().AddRange(arguments)));

    public static ObjectCreationExpressionSyntax WithArgumentList(this ObjectCreationExpressionSyntax declaration, IEnumerable<ArgumentSyntax> arguments)
        => declaration.WithArgumentList(ArgumentList(new SeparatedSyntaxList<ArgumentSyntax>().AddRange(arguments)));

    public static ObjectCreationExpressionSyntax WithArgumentList(this ObjectCreationExpressionSyntax declaration, params ArgumentSyntax[] arguments)
        => declaration.WithArgumentList(ArgumentList(new SeparatedSyntaxList<ArgumentSyntax>().AddRange(arguments)));

    public static ParameterSyntax Parameter(IParameterSymbol parameter, bool ignoreRefKind = false)
    {
        var result = SyntaxFactory.Parameter(Identifier(parameter.Name))
                        .WithType(TypeSyntax(parameter.Type));

        if (parameter.HasExplicitDefaultValue)
            result = result.WithDefault(EqualsValueClause(LiteralExpression(parameter.ExplicitDefaultValue!)));

        if (ignoreRefKind) return result;

        return parameter.RefKind switch
        {
            RefKind.Ref => result.WithModifiers(TokenList(Token(SyntaxKind.RefKeyword))),
            RefKind.In => result.WithModifiers(TokenList(Token(SyntaxKind.InKeyword))),
            RefKind.RefReadOnlyParameter => result.WithModifiers(TokenList(Token(SyntaxKind.RefKeyword), Token(SyntaxKind.ReadOnlyKeyword))),
            RefKind.Out => result.WithModifiers(TokenList(Token(SyntaxKind.OutKeyword))),
            _ => result
        };
    }

    public static LiteralExpressionSyntax LiteralExpression(object value)
        => value switch
        {
            null => SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression),
            string s => SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(s)),
            //..... :(
            //No way to get enums. value is null.
        };

    public static TypeParameterSyntax TypeParameter(ITypeParameterSymbol typeParameter)
    {
        var result = SyntaxFactory.TypeParameter(typeParameter.Name);

        result = typeParameter.Variance switch
        {
            VarianceKind.In => result.WithVarianceKeyword(Token(SyntaxKind.InKeyword)),
            VarianceKind.Out => result.WithVarianceKeyword(Token(SyntaxKind.OutKeyword)),
            _ => result
        };

        return result;
    }

    public static TypeParameterConstraintClauseSyntax TypeParameterConstraintClause(ITypeParameterSymbol typeParameter)
    {
        
        var result = SyntaxFactory.TypeParameterConstraintClause(typeParameter.Name);
        
        if(typeParameter.HasUnmanagedTypeConstraint)
        {
            result = result.AddConstraints(TypeConstraint(
                                                IdentifierName(
                                                    Identifier(
                                                        TriviaList(),
                                                        SyntaxKind.UnmanagedKeyword,
                                                        "unmanaged",
                                                        "unmanaged",
                                                        TriviaList()))));
        }
        else if(typeParameter.HasValueTypeConstraint)
        {
            result = result.AddConstraints(ClassOrStructConstraint(SyntaxKind.StructConstraint));
        }
        else if (typeParameter.HasReferenceTypeConstraint)
        {
            result = result.AddConstraints(ClassOrStructConstraint(SyntaxKind.ClassConstraint));
        }

        if (typeParameter.HasNotNullConstraint)
        {
            result = result.AddConstraints(TypeConstraint(IdentifierName("notnull")));
        }

        foreach (var item in typeParameter.ConstraintTypes)
        {
           result = result.AddConstraints(TypeConstraint(TypeSyntax(item)));
        }

        if(typeParameter.HasConstructorConstraint)
        {
            result = result.AddConstraints(ConstructorConstraint());
        }

        return result;
    }

    public static VariableDeclarationSyntax WithInitializer(this VariableDeclarationSyntax variableDeclarationSyntax, ExpressionSyntax initializer)
    {
        IEnumerable<VariableDeclaratorSyntax> variables = variableDeclarationSyntax.Variables;

        variables = variables.Select(v => v.WithInitializer(EqualsValueClause(initializer)));

        return variableDeclarationSyntax.WithVariables(new SeparatedSyntaxList<VariableDeclaratorSyntax>().AddRange(variables));
    }

    public static VariableDeclarationSyntax WithVariables(this VariableDeclarationSyntax variableDeclarationSyntax, VariableDeclaratorSyntax variable)
    {
        IEnumerable<VariableDeclaratorSyntax> variables = variableDeclarationSyntax.Variables;

        return variableDeclarationSyntax.WithVariables(new SeparatedSyntaxList<VariableDeclaratorSyntax>().Add(variable));
    }

    public static string CamelCase(string value)
        => char.ToLower(value[0]) + value.Substring(1);

    public static SyntaxToken CamelCase(SyntaxToken value)
        => Identifier(CamelCase(value.Text));
}
