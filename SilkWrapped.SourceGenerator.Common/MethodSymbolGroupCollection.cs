using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace SilkWrapped.SourceGenerator;
public class MethodSymbolGroupCollection
{
    private Dictionary<ITypeSymbol, List<IMethodSymbol>> members = new(SymbolEqualityComparer.Default);

    public void Add(ITypeSymbol typeSymbol, IMethodSymbol methodSymbol)
    {
        if(members.TryGetValue(typeSymbol, out var list))
        {
            list.Add(methodSymbol);
        }
        else members.Add(typeSymbol, new List<IMethodSymbol>() { methodSymbol });
    }

    public bool ContainsKey(ITypeSymbol returnType)
    {
        return members.ContainsKey(returnType);
    }

    public IEnumerable<IMethodSymbol> this[ITypeSymbol typeSymbol] 
    {
        get
        {
            if (members.TryGetValue(typeSymbol, out var list))
            {
                return list;
            }

            return Enumerable.Empty<IMethodSymbol>();
        }
    }
}

public static class MethodSymbolGroupCollectionExtensions
{
    public static IEnumerable<IMethodSymbol> OrderByPriority(this IEnumerable<IMethodSymbol> list, Dictionary<ITypeSymbol, int> priority)
        => list.OrderBy(ms => priority[ms.ContainingType]);
}
