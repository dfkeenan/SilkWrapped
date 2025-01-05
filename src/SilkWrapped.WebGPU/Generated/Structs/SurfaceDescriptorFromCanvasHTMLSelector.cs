namespace SilkWrapped.WebGPU;
public partial struct SurfaceDescriptorFromCanvasHTMLSelector
{
    public unsafe string? Selector;
    public unsafe SurfaceDescriptorFromCanvasHTMLSelector(string? selector = null)
    {
        this = default(SurfaceDescriptorFromCanvasHTMLSelector);
        if (selector != null)
        {
            Selector = selector;
        }
    }
}