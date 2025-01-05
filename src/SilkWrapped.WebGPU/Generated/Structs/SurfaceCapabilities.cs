namespace SilkWrapped.WebGPU;
public partial struct SurfaceCapabilities
{
    public unsafe TextureFormat[]? Formats;
    public unsafe PresentMode[]? PresentModes;
    public unsafe CompositeAlphaMode[]? AlphaModes;
    public unsafe SurfaceCapabilities(TextureFormat[]? formats = null, PresentMode[]? presentModes = null, CompositeAlphaMode[]? alphaModes = null)
    {
        this = default(SurfaceCapabilities);
        if (formats != null)
        {
            Formats = formats;
        }

        if (presentModes != null)
        {
            PresentModes = presentModes;
        }

        if (alphaModes != null)
        {
            AlphaModes = alphaModes;
        }
    }
}