using System.Diagnostics;
using System.Text;

namespace SilkWrapped.SourceGenerator.Common;
public static class StringBuilderExtensions
{
    #region AppendJoin

    public static StringBuilder AppendJoin(this StringBuilder stringBuilder, string? separator, params object?[] values)
    {
        separator ??= string.Empty;
        return stringBuilder.AppendJoinCore(separator, values);
    }

    public static StringBuilder AppendJoin<T>(this StringBuilder stringBuilder, string? separator, IEnumerable<T> values)
    {
        separator ??= string.Empty;
        return stringBuilder.AppendJoinCore(separator, values);
    }

    public static StringBuilder AppendJoin(this StringBuilder stringBuilder, string? separator, params string?[] values)
    {
        separator ??= string.Empty;
        return stringBuilder.AppendJoinCore(separator, values);
    }

    private static StringBuilder AppendJoinCore<T>(this StringBuilder stringBuilder, string separator, IEnumerable<T> values)
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        Debug.Assert(values != null);
        using (IEnumerator<T> en = values!.GetEnumerator())
        {
            if (!en.MoveNext())
            {
                return stringBuilder;
            }

            T value = en.Current;
            if (value != null)
            {
                stringBuilder.Append(value.ToString());
            }

            while (en.MoveNext())
            {
                stringBuilder.Append(separator);
                value = en.Current;
                if (value != null)
                {
                    stringBuilder.Append(value.ToString());
                }
            }
        }
        return stringBuilder;
    }

    #endregion
}
