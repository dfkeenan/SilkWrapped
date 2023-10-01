using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Extensions.Dawn;
//using Silk.NET.WebGPU.Extensions.WGPU;
using SilkWrapped.SourceGenerator;

namespace SilkWrapped.WebGPU;

[ApiContainer(typeof(Silk.NET.WebGPU.Instance), HandleTypeNameExclusionPattern = "(Pfn).*|.*Descriptor|InstanceFeatures|Future|SurfaceCapabilities")]
public unsafe partial class ApiContainer
{
    public ApiContainer()
    {
        Core = Silk.NET.WebGPU.WebGPU.GetApi();
        //if (Core.TryGetDeviceExtension(null, out Dawn dawn))
        //{
        //    Dawn = dawn;
        //}
        //if (Core.TryGetDeviceExtension(null, out Wgpu wgpu))
        //{
        //    Wgpu = wgpu;
        //}
    }

    public Silk.NET.WebGPU.WebGPU Core { get; }
    //public Dawn? Dawn { get; set; }
    //public Wgpu? Wgpu { get; set; }
}
