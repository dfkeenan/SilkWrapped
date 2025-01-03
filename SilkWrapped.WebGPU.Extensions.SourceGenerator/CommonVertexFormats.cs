using System.Numerics;

namespace SilkWrapped.WebGPU.Extensions.SourceGenerator;
internal static class CommonVertexFormats
{
    private static readonly Dictionary<string, string> typeMap = [];
    static CommonVertexFormats()
    {
        AddFormat<Vector2>(VertexFormat.Float32x2);
        AddFormat<Vector3>(VertexFormat.Float32x3);
        AddFormat<Vector4>(VertexFormat.Float32x4);
        AddFormat<Int32>(VertexFormat.Sint32);
        AddFormat<UInt32>(VertexFormat.Uint32);
    }

    private static void AddFormat(string type, VertexFormat format)
    {
        typeMap.Add(type, $"{SGNamespaces.SWWebGPU[nameof(VertexFormat)]}.{format}");
    }

    private static void AddFormat<T>(VertexFormat format)
    {
        var type = typeof(T);
        AddFormat($"global::{type.FullName}", format);
    }

    public static bool TryGetFormat(string type, out string format)
    {
        return typeMap.TryGetValue(type, out format);
    }
}
