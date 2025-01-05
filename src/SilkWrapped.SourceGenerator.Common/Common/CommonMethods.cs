namespace SilkWrapped.SourceGenerator.Common;
internal class CommonMethods
{
    public static string SizeOf(string typeName)
        => $"{CommonNamespaces.CompilerServices["Unsafe"]}.SizeOf<{typeName}>()";
}
