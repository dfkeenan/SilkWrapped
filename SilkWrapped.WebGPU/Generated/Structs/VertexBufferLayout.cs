using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct VertexBufferLayout
{
    public ulong ArrayStride;
    public VertexStepMode StepMode;
    public unsafe VertexAttribute[]? Attributes;
    public unsafe VertexBufferLayout(ulong? arrayStride = null, VertexStepMode? stepMode = null, VertexAttribute[]? attributes = null)
    {
        this = default(VertexBufferLayout);
        if (arrayStride.HasValue)
        {
            ArrayStride = arrayStride.Value;
        }

        if (stepMode.HasValue)
        {
            StepMode = stepMode.Value;
        }

        if (attributes != null)
        {
            Attributes = attributes;
        }
    }
}