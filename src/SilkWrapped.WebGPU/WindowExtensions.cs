using Silk.NET.Core.Contexts;

namespace SilkWrapped.WebGPU;
public unsafe static class WindowExtensions
{
    public static Surface CreateWebGPUSurface(this INativeWindowSource view, Instance instance)
    {

        var handle = Silk.NET.WebGPU.WebGPUSurface.CreateWebGPUSurface(view, instance.WebGPU, instance.Handle);

        return new Surface(instance.WebGPU, handle);
    }
}
