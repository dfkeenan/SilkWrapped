namespace SilkWrapped.WebGPU;
public partial struct PrimitiveState
{
    public PrimitiveTopology Topology;
    public IndexFormat StripIndexFormat;
    public FrontFace FrontFace;
    public CullMode CullMode;
    public unsafe PrimitiveState(PrimitiveTopology? topology = null, IndexFormat? stripIndexFormat = null, FrontFace? frontFace = null, CullMode? cullMode = null)
    {
        this = default(PrimitiveState);
        if (topology.HasValue)
        {
            Topology = topology.Value;
        }

        if (stripIndexFormat.HasValue)
        {
            StripIndexFormat = stripIndexFormat.Value;
        }

        if (frontFace.HasValue)
        {
            FrontFace = frontFace.Value;
        }

        if (cullMode.HasValue)
        {
            CullMode = cullMode.Value;
        }
    }
}