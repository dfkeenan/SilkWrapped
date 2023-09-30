using Silk.NET.WebGPU.Extensions.Dawn;
using SilkWrapped.SourceGenerator;

namespace SilkWrapped.WebGPU;

[ApiContainer(typeof(Silk.NET.WebGPU.Instance))]
public unsafe partial class ApiContainer
{
    public ApiContainer()
    {
        Core = Silk.NET.WebGPU.WebGPU.GetApi();
        if(Core.TryGetDeviceExtension(null, out Dawn dawn))
        {
            Dawn = dawn;
        }
    }

    public Silk.NET.WebGPU.WebGPU Core { get; }
    public Dawn? Dawn { get; set; }
}
