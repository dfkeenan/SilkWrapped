using Silk.NET.Core.Contexts;

namespace SilkWrapped.WebGPU;
public unsafe static class WindowExtensions
{
    public static SurfaceWrapper CreateWebGPUSurface(this INativeWindowSource view, InstanceWrapper instance)
    {
        Surface* handle = view.CreateWebGPUSurface(instance.Api.Core, instance);

        return new SurfaceWrapper(instance.Api, handle);
    }
}
