using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct VertexAttribute
{
    public VertexFormat Format;
    public ulong Offset;
    public uint ShaderLocation;
    public VertexAttribute(VertexFormat? format = null, ulong? offset = null, uint? shaderLocation = null)
    {
        this = default(VertexAttribute);
        if (format.HasValue)
        {
            Format = format.Value;
        }

        if (offset.HasValue)
        {
            Offset = offset.Value;
        }

        if (shaderLocation.HasValue)
        {
            ShaderLocation = shaderLocation.Value;
        }
    }
}