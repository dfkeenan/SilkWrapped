namespace SilkWrapped.WebGPU;


[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
public sealed class VertexStructAttribute(VertexStepMode stepMode = VertexStepMode.Vertex) : Attribute
{
    public VertexStepMode StepMode { get; } = stepMode;
}

public interface IVertexStruct
{
    static abstract VertexBufferLayout GetLayout();
}
