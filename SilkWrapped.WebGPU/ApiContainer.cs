using SilkWrapped.SourceGenerator;

namespace SilkWrapped.WebGPU;

[ApiContainer(typeof(Silk.NET.WebGPU.WebGPU), typeof(Silk.NET.WebGPU.Instance))]
[ApiExtension(typeof(Silk.NET.WebGPU.Extensions.Dawn.Dawn))]
public partial class ApiContainer
{

}
