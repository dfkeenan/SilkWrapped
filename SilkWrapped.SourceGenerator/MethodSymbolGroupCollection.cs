using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace SilkWrapped.SourceGenerator;
internal class MethodSymbolGroupCollection
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
