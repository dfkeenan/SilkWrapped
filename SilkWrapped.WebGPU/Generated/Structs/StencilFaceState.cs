namespace SilkWrapped.WebGPU;
public partial struct StencilFaceState
{
    public CompareFunction Compare;
    public StencilOperation FailOp;
    public StencilOperation DepthFailOp;
    public StencilOperation PassOp;
    public StencilFaceState(CompareFunction? compare = null, StencilOperation? failOp = null, StencilOperation? depthFailOp = null, StencilOperation? passOp = null)
    {
        this = default(StencilFaceState);
        if (compare.HasValue)
        {
            Compare = compare.Value;
        }

        if (failOp.HasValue)
        {
            FailOp = failOp.Value;
        }

        if (depthFailOp.HasValue)
        {
            DepthFailOp = depthFailOp.Value;
        }

        if (passOp.HasValue)
        {
            PassOp = passOp.Value;
        }
    }
}