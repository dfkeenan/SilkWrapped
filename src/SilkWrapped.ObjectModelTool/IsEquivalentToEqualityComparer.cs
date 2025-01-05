using System.Diagnostics.CodeAnalysis;

namespace SilkWrapped.ObjectModelTool;

internal class IsEquivalentToEqualityComparer<T> : IEqualityComparer<T>
    where T : SyntaxNode
{
    public bool Equals(T? x, T? y)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;

        return x.IsEquivalentTo(y);
    }

    public int GetHashCode([DisallowNull] T obj)
    {
        return obj.ToString().GetHashCode();
    }
}
