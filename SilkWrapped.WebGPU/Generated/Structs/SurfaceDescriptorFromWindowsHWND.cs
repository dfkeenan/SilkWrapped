using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct SurfaceDescriptorFromWindowsHWND
{
    public unsafe void* Hinstance;
    public unsafe void* Hwnd;
    public unsafe SurfaceDescriptorFromWindowsHWND(void* hinstance = null, void* hwnd = null)
    {
        this = default(SurfaceDescriptorFromWindowsHWND);
        if (hinstance != null)
        {
            Hinstance = hinstance;
        }

        if (hwnd != null)
        {
            Hwnd = hwnd;
        }
    }
}