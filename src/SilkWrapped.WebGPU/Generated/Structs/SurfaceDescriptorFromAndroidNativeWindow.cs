namespace SilkWrapped.WebGPU;
public partial struct SurfaceDescriptorFromAndroidNativeWindow
{
    public unsafe void* Window;
    public unsafe SurfaceDescriptorFromAndroidNativeWindow(void* window = null)
    {
        this = default(SurfaceDescriptorFromAndroidNativeWindow);
        if (window != null)
        {
            Window = window;
        }
    }
}