namespace SilkWrapped.WebGPU;
public partial struct Limits
{
    public uint MaxTextureDimension1D;
    public uint MaxTextureDimension2D;
    public uint MaxTextureDimension3D;
    public uint MaxTextureArrayLayers;
    public uint MaxBindGroups;
    public uint MaxBindGroupsPlusVertexBuffers;
    public uint MaxBindingsPerBindGroup;
    public uint MaxDynamicUniformBuffersPerPipelineLayout;
    public uint MaxDynamicStorageBuffersPerPipelineLayout;
    public uint MaxSampledTexturesPerShaderStage;
    public uint MaxSamplersPerShaderStage;
    public uint MaxStorageBuffersPerShaderStage;
    public uint MaxStorageTexturesPerShaderStage;
    public uint MaxUniformBuffersPerShaderStage;
    public ulong MaxUniformBufferBindingSize;
    public ulong MaxStorageBufferBindingSize;
    public uint MinUniformBufferOffsetAlignment;
    public uint MinStorageBufferOffsetAlignment;
    public uint MaxVertexBuffers;
    public ulong MaxBufferSize;
    public uint MaxVertexAttributes;
    public uint MaxVertexBufferArrayStride;
    public uint MaxInterStageShaderComponents;
    public uint MaxInterStageShaderVariables;
    public uint MaxColorAttachments;
    public uint MaxColorAttachmentBytesPerSample;
    public uint MaxComputeWorkgroupStorageSize;
    public uint MaxComputeInvocationsPerWorkgroup;
    public uint MaxComputeWorkgroupSizeX;
    public uint MaxComputeWorkgroupSizeY;
    public uint MaxComputeWorkgroupSizeZ;
    public uint MaxComputeWorkgroupsPerDimension;
    public Limits(uint? maxTextureDimension1D = null, uint? maxTextureDimension2D = null, uint? maxTextureDimension3D = null, uint? maxTextureArrayLayers = null, uint? maxBindGroups = null, uint? maxBindGroupsPlusVertexBuffers = null, uint? maxBindingsPerBindGroup = null, uint? maxDynamicUniformBuffersPerPipelineLayout = null, uint? maxDynamicStorageBuffersPerPipelineLayout = null, uint? maxSampledTexturesPerShaderStage = null, uint? maxSamplersPerShaderStage = null, uint? maxStorageBuffersPerShaderStage = null, uint? maxStorageTexturesPerShaderStage = null, uint? maxUniformBuffersPerShaderStage = null, ulong? maxUniformBufferBindingSize = null, ulong? maxStorageBufferBindingSize = null, uint? minUniformBufferOffsetAlignment = null, uint? minStorageBufferOffsetAlignment = null, uint? maxVertexBuffers = null, ulong? maxBufferSize = null, uint? maxVertexAttributes = null, uint? maxVertexBufferArrayStride = null, uint? maxInterStageShaderComponents = null, uint? maxInterStageShaderVariables = null, uint? maxColorAttachments = null, uint? maxColorAttachmentBytesPerSample = null, uint? maxComputeWorkgroupStorageSize = null, uint? maxComputeInvocationsPerWorkgroup = null, uint? maxComputeWorkgroupSizeX = null, uint? maxComputeWorkgroupSizeY = null, uint? maxComputeWorkgroupSizeZ = null, uint? maxComputeWorkgroupsPerDimension = null)
    {
        this = default(Limits);
        if (maxTextureDimension1D.HasValue)
        {
            MaxTextureDimension1D = maxTextureDimension1D.Value;
        }

        if (maxTextureDimension2D.HasValue)
        {
            MaxTextureDimension2D = maxTextureDimension2D.Value;
        }

        if (maxTextureDimension3D.HasValue)
        {
            MaxTextureDimension3D = maxTextureDimension3D.Value;
        }

        if (maxTextureArrayLayers.HasValue)
        {
            MaxTextureArrayLayers = maxTextureArrayLayers.Value;
        }

        if (maxBindGroups.HasValue)
        {
            MaxBindGroups = maxBindGroups.Value;
        }

        if (maxBindGroupsPlusVertexBuffers.HasValue)
        {
            MaxBindGroupsPlusVertexBuffers = maxBindGroupsPlusVertexBuffers.Value;
        }

        if (maxBindingsPerBindGroup.HasValue)
        {
            MaxBindingsPerBindGroup = maxBindingsPerBindGroup.Value;
        }

        if (maxDynamicUniformBuffersPerPipelineLayout.HasValue)
        {
            MaxDynamicUniformBuffersPerPipelineLayout = maxDynamicUniformBuffersPerPipelineLayout.Value;
        }

        if (maxDynamicStorageBuffersPerPipelineLayout.HasValue)
        {
            MaxDynamicStorageBuffersPerPipelineLayout = maxDynamicStorageBuffersPerPipelineLayout.Value;
        }

        if (maxSampledTexturesPerShaderStage.HasValue)
        {
            MaxSampledTexturesPerShaderStage = maxSampledTexturesPerShaderStage.Value;
        }

        if (maxSamplersPerShaderStage.HasValue)
        {
            MaxSamplersPerShaderStage = maxSamplersPerShaderStage.Value;
        }

        if (maxStorageBuffersPerShaderStage.HasValue)
        {
            MaxStorageBuffersPerShaderStage = maxStorageBuffersPerShaderStage.Value;
        }

        if (maxStorageTexturesPerShaderStage.HasValue)
        {
            MaxStorageTexturesPerShaderStage = maxStorageTexturesPerShaderStage.Value;
        }

        if (maxUniformBuffersPerShaderStage.HasValue)
        {
            MaxUniformBuffersPerShaderStage = maxUniformBuffersPerShaderStage.Value;
        }

        if (maxUniformBufferBindingSize.HasValue)
        {
            MaxUniformBufferBindingSize = maxUniformBufferBindingSize.Value;
        }

        if (maxStorageBufferBindingSize.HasValue)
        {
            MaxStorageBufferBindingSize = maxStorageBufferBindingSize.Value;
        }

        if (minUniformBufferOffsetAlignment.HasValue)
        {
            MinUniformBufferOffsetAlignment = minUniformBufferOffsetAlignment.Value;
        }

        if (minStorageBufferOffsetAlignment.HasValue)
        {
            MinStorageBufferOffsetAlignment = minStorageBufferOffsetAlignment.Value;
        }

        if (maxVertexBuffers.HasValue)
        {
            MaxVertexBuffers = maxVertexBuffers.Value;
        }

        if (maxBufferSize.HasValue)
        {
            MaxBufferSize = maxBufferSize.Value;
        }

        if (maxVertexAttributes.HasValue)
        {
            MaxVertexAttributes = maxVertexAttributes.Value;
        }

        if (maxVertexBufferArrayStride.HasValue)
        {
            MaxVertexBufferArrayStride = maxVertexBufferArrayStride.Value;
        }

        if (maxInterStageShaderComponents.HasValue)
        {
            MaxInterStageShaderComponents = maxInterStageShaderComponents.Value;
        }

        if (maxInterStageShaderVariables.HasValue)
        {
            MaxInterStageShaderVariables = maxInterStageShaderVariables.Value;
        }

        if (maxColorAttachments.HasValue)
        {
            MaxColorAttachments = maxColorAttachments.Value;
        }

        if (maxColorAttachmentBytesPerSample.HasValue)
        {
            MaxColorAttachmentBytesPerSample = maxColorAttachmentBytesPerSample.Value;
        }

        if (maxComputeWorkgroupStorageSize.HasValue)
        {
            MaxComputeWorkgroupStorageSize = maxComputeWorkgroupStorageSize.Value;
        }

        if (maxComputeInvocationsPerWorkgroup.HasValue)
        {
            MaxComputeInvocationsPerWorkgroup = maxComputeInvocationsPerWorkgroup.Value;
        }

        if (maxComputeWorkgroupSizeX.HasValue)
        {
            MaxComputeWorkgroupSizeX = maxComputeWorkgroupSizeX.Value;
        }

        if (maxComputeWorkgroupSizeY.HasValue)
        {
            MaxComputeWorkgroupSizeY = maxComputeWorkgroupSizeY.Value;
        }

        if (maxComputeWorkgroupSizeZ.HasValue)
        {
            MaxComputeWorkgroupSizeZ = maxComputeWorkgroupSizeZ.Value;
        }

        if (maxComputeWorkgroupsPerDimension.HasValue)
        {
            MaxComputeWorkgroupsPerDimension = maxComputeWorkgroupsPerDimension.Value;
        }
    }
}