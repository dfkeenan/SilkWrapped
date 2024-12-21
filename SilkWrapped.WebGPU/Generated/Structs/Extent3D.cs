using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct Extent3D
{
    public uint Width;
    public uint Height;
    public uint DepthOrArrayLayers;
    public Extent3D(uint? width = null, uint? height = null, uint? depthOrArrayLayers = null)
    {
        this = default(Extent3D);
        if (width.HasValue)
        {
            Width = width.Value;
        }

        if (height.HasValue)
        {
            Height = height.Value;
        }

        if (depthOrArrayLayers.HasValue)
        {
            DepthOrArrayLayers = depthOrArrayLayers.Value;
        }
    }
}