using Silk.NET.Core.Contexts;

namespace SilkWrapped.WebGPU;
public unsafe static class WindowExtensions
{
    public static SurfaceWrapper CreateWebGPUSurface(this INativeWindowSource view, InstanceWrapper instance)
    {
        
        var handle = Silk.NET.WebGPU.WebGPUSurface.CreateWebGPUSurface(view, instance.WebGPU, instance.Handle);

        return new SurfaceWrapper(instance.WebGPU, handle);
    }
}
