using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct SurfaceDescriptorFromWaylandSurface
{
    public unsafe void* Display;
    public unsafe void* Surface;
    public unsafe SurfaceDescriptorFromWaylandSurface(void* display = null, void* surface = null)
    {
        this = default(SurfaceDescriptorFromWaylandSurface);
        if (display != null)
        {
            Display = display;
        }

        if (surface != null)
        {
            Surface = surface;
        }
    }
}