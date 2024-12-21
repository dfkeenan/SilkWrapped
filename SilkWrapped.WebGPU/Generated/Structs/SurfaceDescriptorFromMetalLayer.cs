namespace SilkWrapped.WebGPU;
public partial struct SurfaceDescriptorFromMetalLayer
{
    public unsafe void* Layer;
    public unsafe SurfaceDescriptorFromMetalLayer(void* layer = null)
    {
        this = default(SurfaceDescriptorFromMetalLayer);
        if (layer != null)
        {
            Layer = layer;
        }
    }
}