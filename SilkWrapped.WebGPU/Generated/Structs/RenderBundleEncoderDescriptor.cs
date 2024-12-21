using Silk.NET.Core;
using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct RenderBundleEncoderDescriptor
{
    public unsafe string? Label;
    public unsafe TextureFormat[]? ColorFormats;
    public TextureFormat DepthStencilFormat;
    public uint SampleCount;
    public Bool32 DepthReadOnly;
    public Bool32 StencilReadOnly;
    public unsafe RenderBundleEncoderDescriptor(string? label = null, TextureFormat[]? colorFormats = null, TextureFormat? depthStencilFormat = null, uint? sampleCount = null, Bool32? depthReadOnly = null, Bool32? stencilReadOnly = null)
    {
        this = default(RenderBundleEncoderDescriptor);
        if (label != null)
        {
            Label = label;
        }

        if (colorFormats != null)
        {
            ColorFormats = colorFormats;
        }

        if (depthStencilFormat.HasValue)
        {
            DepthStencilFormat = depthStencilFormat.Value;
        }

        if (sampleCount.HasValue)
        {
            SampleCount = sampleCount.Value;
        }

        if (depthReadOnly.HasValue)
        {
            DepthReadOnly = depthReadOnly.Value;
        }

        if (stencilReadOnly.HasValue)
        {
            StencilReadOnly = stencilReadOnly.Value;
        }
    }
}