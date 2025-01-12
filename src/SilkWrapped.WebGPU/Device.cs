namespace SilkWrapped.WebGPU;
public unsafe partial class Device
{
    public ShaderModule CreateShaderModuleWGSL(string code)
    {
        var wgslDescriptor = new Silk.NET.WebGPU.ShaderModuleWGSLDescriptor
        {
            Code = (byte*)SilkMarshal.StringToPtr(code, NativeStringEncoding.UTF8),
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

    public ShaderModule CreateShaderModuleWGSL(ReadOnlySpan<byte> code)
    {
        fixed (byte* codePtr = code)
        {
            var wgslDescriptor = new Silk.NET.WebGPU.ShaderModuleWGSLDescriptor
            {
                Code = codePtr,
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

            return new ShaderModule(WebGPU, result);

        }
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

    public Texture CreateTexture(
        uint width,
        uint height,
        TextureFormat format,
        TextureUsage usage,
        uint sampleCount = 1,
        uint mipLevelCount = 1,
        params ReadOnlySpan<TextureFormat> viewFormats)
    {
        var description = new TextureDescriptor
        {
            Size = new Extent3D(width, height, 1),
            Format = format,
            Usage = usage,
            Dimension = TextureDimension.Dimension2D,
            SampleCount = sampleCount,
            MipLevelCount = mipLevelCount,
            ViewFormats = viewFormats.Length > 0 ? viewFormats.ToArray() : null,
        };

        return CreateTexture(in description);
    }

    //public IEnumerable<FeatureName> EnumerateFeatures()
    //{
    //    //var featureCount = EnumerateFeatures(ref Unsafe.NullRef<FeatureName>());
    //    //var featureNames = new FeatureName[featureCount];
    //    //EnumerateFeatures(ref featureNames[0]);
    //    //return featureNames;
    //    //ref Silk.NET.WebGPU.FeatureName featureNames;
    //    var featureCount = WebGPU.DeviceEnumerateFeatures(Handle, null);


    //    return null;
    //}

    partial void Disposing()
    {

    }

    partial void Disposed()
    {

    }
}
