namespace SilkWrapped.WebGPU;


[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
public sealed class VertexStructAttribute(VertexStepMode stepMode = VertexStepMode.Vertex) : Attribute
{
    public VertexStepMode StepMode { get; } = stepMode;
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class VertexFormatAttribute(VertexFormat vertexFormat) : Attribute
{
    public VertexFormat VertexFormat { get; } = vertexFormat;
}

public interface IVertexStruct
{
    static abstract VertexBufferLayout GetLayout();
}
