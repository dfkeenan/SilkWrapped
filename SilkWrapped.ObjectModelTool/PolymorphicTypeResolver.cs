using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.CodeAnalysis.CSharp;

namespace SilkWrapped.ObjectModelTool;
internal class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
{
    private static readonly Dictionary<Type, IEnumerable<Type>> derivedTypes = [];

    static PolymorphicTypeResolver()
    {
        var types = typeof(PolymorphicTypeResolver).Assembly.GetTypes()
                        .Where(t => t is { IsClass: true, IsAbstract: false });

        derivedTypes[typeof(GeneratorTransformBase)]
            = types.Where(t => t.IsAssignableTo(typeof(GeneratorTransformBase))).ToList();

        derivedTypes[typeof(CSharpSyntaxRewriter)]
            = types.Where(t => t.IsAssignableTo(typeof(CSharpSyntaxRewriter))).ToList();
    }

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

        if (derivedTypes.TryGetValue(jsonTypeInfo.Type, out var types))
        {
            jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "$type",
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
            };

            foreach (var item in types)
            {
                jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(item, item.Name));
            }
        }
        return jsonTypeInfo;
    }


}