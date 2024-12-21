using System.Diagnostics.CodeAnalysis;

namespace SilkWrapped.ObjectModelTool;
internal static class SymbolExtensions
{
    public static bool Is(this ITypeSymbol symbol, ITypeSymbol baseType)
    {
        return GetTypes(symbol).Any(t => t.Equals(baseType, SymbolEqualityComparer.Default));

        static IEnumerable<ITypeSymbol> GetTypes(ITypeSymbol type)
        {

            while (type != null)
            {
                yield return type;
                type = type.BaseType!;
            }
        }
    }

    public static bool Is(this ITypeSymbol symbol, string baseType)
    {
        return GetTypes(symbol).Any(t => t.ToString() == baseType);

        static IEnumerable<ITypeSymbol> GetTypes(ITypeSymbol type)
        {

            while (type != null)
            {
                yield return type;
                type = type.BaseType!;
            }
        }
    }

    public static bool IsNullableOfT(this ITypeSymbol type, [NotNullWhen(true)] out INamedTypeSymbol? outType)
    {
        if (type is not INamedTypeSymbol namedTypeSymbol)
        {
            outType = null;
            return false;
        }


        if (namedTypeSymbol.Name == "Nullable" && namedTypeSymbol.TypeArguments is { Length: 1 } && namedTypeSymbol.TypeArguments[0] is INamedTypeSymbol result)
        {
            outType = result;
            return true;
        }

        outType = null;
        return false;
    }
}
