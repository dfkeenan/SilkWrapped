using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct SurfaceDescriptorFromXlibWindow
{
    public unsafe void* Display;
    public ulong Window;
    public unsafe SurfaceDescriptorFromXlibWindow(void* display = null, ulong? window = null)
    {
        this = default(SurfaceDescriptorFromXlibWindow);
        if (display != null)
        {
            Display = display;
        }

        if (window.HasValue)
        {
            Window = window.Value;
        }
    }
}