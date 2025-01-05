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

    public PipelineLayout CreatePipelineLayout(string label, params ReadOnlySpan<BindGroupLayoutHandle> bindGroupLayouts)
    {
        using var m = new MarshalHelper();
        fixed (BindGroupLayoutHandle* bindGroupLayoutsPtr = bindGroupLayouts)
        {
            Silk.NET.WebGPU.PipelineLayoutDescriptor __descriptor = new()
            {
                Label = m.RentUtf8Ptr(label),
                BindGroupLayoutCount = (nuint)bindGroupLayouts.Length,
                BindGroupLayouts = (Silk.NET.WebGPU.BindGroupLayout**)bindGroupLayoutsPtr,
            };

            var result = WebGPU.DeviceCreatePipelineLayout(Handle, in __descriptor);
            return new PipelineLayout(WebGPU, result);
        }
    }

    public PipelineLayout CreatePipelineLayout(params ReadOnlySpan<BindGroupLayoutHandle> bindGroupLayouts)
        => CreatePipelineLayout(null!, bindGroupLayouts);

    public Sampler CreateSampler(
        FilterMode filter = FilterMode.Linear,
        MipmapFilterMode mipmapFilter = MipmapFilterMode.Linear,
        AddressMode addressMode = AddressMode.Repeat,
        ushort maxAnsiotropy = 16,
        string? label = null)
    {
        var descriptor = new SamplerDescriptor
        {
            Compare = CompareFunction.Undefined,
            MipmapFilter = mipmapFilter,
            MagFilter = filter,
            MinFilter = filter,
            AddressModeU = addressMode,
            AddressModeV = addressMode,
            MaxAnisotropy = maxAnsiotropy,
            Label = label
        };

        return CreateSampler(in descriptor);
    }

    partial void Disposing()
    {

    }

    partial void Disposed()
    {

    }
}
