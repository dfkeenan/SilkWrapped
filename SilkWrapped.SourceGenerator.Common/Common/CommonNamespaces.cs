namespace SilkWrapped.SourceGenerator.Common;
public static class CommonNamespaces
{
    public static readonly TypeNameBuilder CompilerServices = new("System.Runtime.CompilerServices");
    public static readonly TypeNameBuilder Compiler = new("System.CodeDom.Compiler");
    public static readonly TypeNameBuilder Diagnostics = new("System.Diagnostics");
    public static readonly TypeNameBuilder ComponentModel = new("System.ComponentModel");
}

public class TypeNameBuilder(string @namespace)
{
    public string this[string typeName]
    {
        get => $"global::{@namespace}.{typeName}";
    }
}