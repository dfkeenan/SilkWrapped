namespace SilkWrapped.WebGPU;
public partial struct RenderPipelineDescriptor
{
    public unsafe string? Label;
    public unsafe PipelineLayoutHandle Layout;
    public VertexState Vertex;
    public PrimitiveState Primitive;
    public unsafe DepthStencilState? DepthStencil;
    public MultisampleState Multisample;
    public unsafe FragmentState? Fragment;
    public unsafe RenderPipelineDescriptor(string? label = null, PipelineLayoutHandle? layout = null, VertexState? vertex = null, PrimitiveState? primitive = null, DepthStencilState? depthStencil = null, MultisampleState? multisample = null, FragmentState? fragment = null)
    {
        this = default(RenderPipelineDescriptor);
        if (label != null)
        {
            Label = label;
        }

        if (layout.HasValue)
        {
            Layout = layout.Value;
        }

        if (vertex.HasValue)
        {
            Vertex = vertex.Value;
        }

        if (primitive.HasValue)
        {
            Primitive = primitive.Value;
        }

        if (depthStencil.HasValue)
        {
            DepthStencil = depthStencil.Value;
        }

        if (multisample.HasValue)
        {
            Multisample = multisample.Value;
        }

        if (fragment.HasValue)
        {
            Fragment = fragment.Value;
        }
    }
}