using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Contexts;
using Silk.NET.WebGPU;

namespace SilkWrapped.WebGPU;
public unsafe static class WindowExtensions
{
    public static SurfaceWrapper CreateWebGPUSurface(this INativeWindowSource view, InstanceWrapper instance)
    {
        Surface* handle = view.CreateWebGPUSurface(instance.Api.Core, instance);

        return new SurfaceWrapper(instance.Api, handle);
    }
}
