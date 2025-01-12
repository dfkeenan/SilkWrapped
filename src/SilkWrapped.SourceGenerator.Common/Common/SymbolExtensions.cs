using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;

namespace SilkWrapped.SourceGenerator.Common;
public static class SymbolExtensions
{

    public static string GetFullyQualifiedMetadataName(this ITypeSymbol symbol)
    {
        var sb = new StringBuilder();

        Build(symbol, sb);

        return sb.ToString();

        static void Build(ISymbol symbol, StringBuilder builder)
        {
            switch (symbol)
            {
                // Namespaces that are nested also append a leading '.'
                case INamespaceSymbol { ContainingNamespace.IsGlobalNamespace: false }:
                    Build(symbol.ContainingNamespace, builder);
                    builder.Append('.');
                    builder.Append(symbol.MetadataName);
                    break;

                // Other namespaces (ie. the one right before global) skip the leading '.'
                case INamespaceSymbol { IsGlobalNamespace: false }:
                    builder.Append(symbol.MetadataName);
                    break;

                // Types with no namespace just have their metadata name directly written
                case ITypeSymbol { ContainingSymbol: INamespaceSymbol { IsGlobalNamespace: true } }:
                    builder.Append(symbol.MetadataName);
                    break;

                // Types with a containing non-global namespace also append a leading '.'
                case ITypeSymbol { ContainingSymbol: INamespaceSymbol namespaceSymbol }:
                    Build(namespaceSymbol, builder);
                    builder.Append('.');
                    builder.Append(symbol.MetadataName);
                    break;

                // Nested types append a leading '+'
                case ITypeSymbol { ContainingSymbol: ITypeSymbol typeSymbol }:
                    Build(typeSymbol, builder);
                    builder.Append('+');
                    builder.Append(symbol.MetadataName);
                    break;
                default:
                    break;
            }
        }
    }

    public static bool ImplementsInterface(this ITypeSymbol typeSymbol, string interfaceMetadataName)
    {
        for (int i = 0; i < typeSymbol.Interfaces.Length; i++)
        {
            if (typeSymbol.Interfaces[i].GetFullyQualifiedMetadataName() == interfaceMetadataName) return true;
        }
        return false;
    }

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
