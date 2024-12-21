using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct SurfaceDescriptorFromXcbWindow
{
    public unsafe void* Connection;
    public uint Window;
    public unsafe SurfaceDescriptorFromXcbWindow(void* connection = null, uint? window = null)
    {
        this = default(SurfaceDescriptorFromXcbWindow);
        if (connection != null)
        {
            Connection = connection;
        }

        if (window.HasValue)
        {
            Window = window.Value;
        }
    }
}