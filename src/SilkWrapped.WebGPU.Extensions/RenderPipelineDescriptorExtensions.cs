using System.Runtime.CompilerServices;

namespace SilkWrapped.WebGPU;
public static class RenderPipelineDescriptorExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RenderPipelineDescriptor WithLayout(
        this in RenderPipelineDescriptor descriptor,
        PipelineLayoutHandle layoutHandle)
    {
        return descriptor with
        {
            Layout = layoutHandle,
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RenderPipelineDescriptor WithVertex(
        this in RenderPipelineDescriptor descriptor,
        ShaderModuleHandle module,
        string entryPoint,
        params ReadOnlySpan<VertexBufferLayout> buffers)
    {
        return descriptor with
        {
            Vertex = new VertexState()
            {
                Module = module,
                EntryPoint = entryPoint,
                Buffers = buffers.Length > 0 ? buffers.ToArray() : null
            }
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RenderPipelineDescriptor WithVertex<TVertex>(
        this in RenderPipelineDescriptor descriptor,
        ShaderModuleHandle module,
        string entryPoint)
        where TVertex : unmanaged, IVertexStruct
    {
        return descriptor.WithVertex(module, entryPoint, [TVertex.GetLayout()]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RenderPipelineDescriptor WithPrimitive(
        this in RenderPipelineDescriptor descriptor,
        PrimitiveTopology topology,
        CullMode cullMode = CullMode.None,
        FrontFace frontFace = FrontFace.CW,
        IndexFormat stripIndexForm = IndexFormat.Undefined)
    {
        return descriptor with
        {
            Primitive = new PrimitiveState()
            {
                Topology = topology,
                CullMode = cullMode,
                FrontFace = frontFace,
                StripIndexFormat = stripIndexForm
            }
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RenderPipelineDescriptor WithMultisampleState(
        this in RenderPipelineDescriptor descriptor,
        uint count = 1,
        uint mask = ~0u,
        bool alphaToCoverageEnabled = false)
    {
        return descriptor with
        {
            Multisample = new MultisampleState
            {
                Count = count,
                Mask = mask,
                AlphaToCoverageEnabled = alphaToCoverageEnabled
            }
        };
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RenderPipelineDescriptor WithFragment(
        this in RenderPipelineDescriptor descriptor,
        ShaderModuleHandle module,
        string entryPoint,
        params ReadOnlySpan<ColorTargetState> targets)
    {
        return descriptor with
        {
            Fragment = new FragmentState
            {
                Module = module,
                EntryPoint = entryPoint,
                Targets = targets.Length > 0 ? targets.ToArray() : null
            }
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RenderPipelineDescriptor WithFragment(
        this in RenderPipelineDescriptor descriptor,
        ShaderModuleHandle module,
        string entryPoint,
        TextureFormat targetFormat,
        BlendState targetBlend,
        ColorWriteMask targetWriteMask = ColorWriteMask.All)
    {
        return descriptor with
        {
            Fragment = new FragmentState
            {
                Module = module,
                EntryPoint = entryPoint,
                Targets =
                [
                    new ColorTargetState
                    {
                        Format = targetFormat,
                        Blend = targetBlend,
                        WriteMask = targetWriteMask
                    }
                ]
            }
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RenderPipelineDescriptor WithDepthStencil(
        this in RenderPipelineDescriptor descriptor,
        TextureFormat format,
        CompareFunction depthCompare,
        bool depthWriteEnabled = true,
        StencilFaceState? stencilFront = null,
        StencilFaceState? stencilBack = null)
    {
        return descriptor with
        {
            DepthStencil = new DepthStencilState()
            {
                DepthWriteEnabled = depthWriteEnabled,
                DepthCompare = depthCompare,
                Format = format,

                StencilFront = stencilFront ?? new StencilFaceState()
                {
                    Compare = CompareFunction.Never,
                },
                StencilBack = stencilBack ?? new StencilFaceState()
                {
                    Compare = CompareFunction.Never,
                }
            }
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RenderPipeline Create(
        this in RenderPipelineDescriptor descriptor,
        Device device)
    {
        return device.CreateRenderPipeline(descriptor);
    }
}
