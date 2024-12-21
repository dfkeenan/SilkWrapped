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

    public unsafe Buffer CreateBuffer<T>(BufferUsage usage, ulong length = 1, bool mappedAtCreation = false)
        where T : unmanaged
    {
        var descriptor = new BufferDescriptor
        {
            Size = (ulong)sizeof(T) * length,
            Usage = usage,
            MappedAtCreation = mappedAtCreation
        };

        return CreateBuffer(in descriptor);
    }

    partial void Disposing()
    {

    }

    partial void Disposed()
    {

    }
}
