namespace SilkWrapped.WebGPU;
public unsafe partial class Device
{
    public ShaderModule CreateShaderModuleWGSL(string code)
    {
        var wgslDescriptor = new Silk.NET.WebGPU.ShaderModuleWGSLDescriptor
        {
            Code = (byte*)SilkMarshal.StringToPtr(code),
            Chain = new Silk.NET.WebGPU.ChainedStruct
            {
                SType = Silk.NET.WebGPU.SType.ShaderModuleWgslDescriptor
            }
        };

        var shaderModuleDescriptor = new Silk.NET.WebGPU.ShaderModuleDescriptor
        {
            NextInChain = (Silk.NET.WebGPU.ChainedStruct*)(&wgslDescriptor),
        };

        var result = WebGPU.DeviceCreateShaderModule(Handle, in shaderModuleDescriptor);

        SilkMarshal.FreeString((nint)wgslDescriptor.Code);

        return new ShaderModule(WebGPU, result);
    }

    partial void Disposing()
    {

    }

    partial void Disposed()
    {

    }
}
