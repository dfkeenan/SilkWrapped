using Silk.NET.Core;
using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct DepthStencilState
{
    public TextureFormat Format;
    public Bool32 DepthWriteEnabled;
    public CompareFunction DepthCompare;
    public StencilFaceState StencilFront;
    public StencilFaceState StencilBack;
    public uint StencilReadMask;
    public uint StencilWriteMask;
    public int DepthBias;
    public float DepthBiasSlopeScale;
    public float DepthBiasClamp;
    public unsafe DepthStencilState(TextureFormat? format = null, Bool32? depthWriteEnabled = null, CompareFunction? depthCompare = null, StencilFaceState? stencilFront = null, StencilFaceState? stencilBack = null, uint? stencilReadMask = null, uint? stencilWriteMask = null, int? depthBias = null, float? depthBiasSlopeScale = null, float? depthBiasClamp = null)
    {
        this = default(DepthStencilState);
        if (format.HasValue)
        {
            Format = format.Value;
        }

        if (depthWriteEnabled.HasValue)
        {
            DepthWriteEnabled = depthWriteEnabled.Value;
        }

        if (depthCompare.HasValue)
        {
            DepthCompare = depthCompare.Value;
        }

        if (stencilFront.HasValue)
        {
            StencilFront = stencilFront.Value;
        }

        if (stencilBack.HasValue)
        {
            StencilBack = stencilBack.Value;
        }

        if (stencilReadMask.HasValue)
        {
            StencilReadMask = stencilReadMask.Value;
        }

        if (stencilWriteMask.HasValue)
        {
            StencilWriteMask = stencilWriteMask.Value;
        }

        if (depthBias.HasValue)
        {
            DepthBias = depthBias.Value;
        }

        if (depthBiasSlopeScale.HasValue)
        {
            DepthBiasSlopeScale = depthBiasSlopeScale.Value;
        }

        if (depthBiasClamp.HasValue)
        {
            DepthBiasClamp = depthBiasClamp.Value;
        }
    }
}