using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace SilkWrapped.SourceGenerator;
public static class SymbolExtentions
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
}
