namespace SilkWrapped.ObjectModelTool;

internal abstract class GeneratorTransformBase
{
    public abstract Task TransformAsync(GeneratorTransformContext context, CancellationToken cancellationToken);

    protected static string? TypeName(TypeSyntax? type)
    {
        if (type is IdentifierNameSyntax { Identifier.Text: string name }) return name;

        if (type is PointerTypeSyntax pointerType) return TypeName(pointerType.ElementType);
        if (type is NullableTypeSyntax nullableType) return TypeName(nullableType.ElementType);
        if (type is QualifiedNameSyntax qualifiedType) return TypeName(qualifiedType.Right);

        return null;
    }

    public override string ToString()
    {
        return GetType().Name;
    }
}