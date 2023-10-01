namespace SilkWrapped.WebGPU;
public unsafe partial class DeviceWrapper
{
    public ShaderModuleWrapper CreateShaderModuleWGSL(string code)
    {
        var wgslDescriptor = new ShaderModuleWGSLDescriptor
        {
            Code = (byte*)SilkMarshal.StringToPtr(code),
            Chain = new ChainedStruct
            {
                SType = SType.ShaderModuleWgslDescriptor
            }
        };

        var shaderModuleDescriptor = new ShaderModuleDescriptor
        {
            NextInChain = (ChainedStruct*)(&wgslDescriptor),
        };

        var result = CreateShaderModule(shaderModuleDescriptor);

        SilkMarshal.FreeString((nint)wgslDescriptor.Code);

        return result;
    }
}
