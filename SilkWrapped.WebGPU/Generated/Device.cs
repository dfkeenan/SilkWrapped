using System.Runtime.CompilerServices;
using Silk.NET.Core;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct DeviceHandle
{
    private readonly Silk.NET.WebGPU.Device* nativeHandle;
    private DeviceHandle(Silk.NET.WebGPU.Device* nativeHandle)
    {
        this.nativeHandle = nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.Device*(DeviceHandle handle) => handle.nativeHandle;
    public static implicit operator DeviceHandle(Silk.NET.WebGPU.Device* handle) => new DeviceHandle(handle);
}

public unsafe partial class Device : System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public DeviceHandle Handle { get; private set; }

    public Device(Silk.NET.WebGPU.WebGPU webGPU, DeviceHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator DeviceHandle(Device obj) => obj.Handle;
    public unsafe BindGroup CreateBindGroup(in BindGroupDescriptor descriptor)
    {
        using var m = new MarshalHelper();
        Silk.NET.WebGPU.BindGroupDescriptor __descriptor = default;
        __descriptor.Label = m.RentUtf8Ptr(descriptor.Label);
        __descriptor.Layout = descriptor.Layout;
        if (descriptor.Entries != null)
        {
            __descriptor.EntryCount = (nuint)descriptor.Entries!.Length;
            __descriptor.Entries = MarshalHelper.AsPointer(descriptor.Entries!.Length <= MarshalHelper.MaxStack ? stackalloc Silk.NET.WebGPU.BindGroupEntry[descriptor.Entries!.Length] : m.RentSpan<Silk.NET.WebGPU.BindGroupEntry>(descriptor.Entries!.Length));
            for (int i = 0; i < descriptor.Entries!.Length; i++)
            {
                __descriptor.Entries[i].Binding = descriptor.Entries![i].Binding;
                __descriptor.Entries[i].Buffer = descriptor.Entries![i].Buffer;
                __descriptor.Entries[i].Offset = descriptor.Entries![i].Offset;
                __descriptor.Entries[i].Size = descriptor.Entries![i].Size;
                __descriptor.Entries[i].Sampler = descriptor.Entries![i].Sampler;
                __descriptor.Entries[i].TextureView = descriptor.Entries![i].TextureView;
            }
        }

        var result = WebGPU.DeviceCreateBindGroup(Handle, in __descriptor);
        return new BindGroup(WebGPU, result);
    }

    public unsafe BindGroupLayout CreateBindGroupLayout(in BindGroupLayoutDescriptor descriptor)
    {
        using var m = new MarshalHelper();
        Silk.NET.WebGPU.BindGroupLayoutDescriptor __descriptor = default;
        __descriptor.Label = m.RentUtf8Ptr(descriptor.Label);
        if (descriptor.Entries != null)
        {
            __descriptor.EntryCount = (nuint)descriptor.Entries!.Length;
            __descriptor.Entries = MarshalHelper.AsPointer(descriptor.Entries!.Length <= MarshalHelper.MaxStack ? stackalloc Silk.NET.WebGPU.BindGroupLayoutEntry[descriptor.Entries!.Length] : m.RentSpan<Silk.NET.WebGPU.BindGroupLayoutEntry>(descriptor.Entries!.Length));
            for (int i = 0; i < descriptor.Entries!.Length; i++)
            {
                __descriptor.Entries[i].Binding = descriptor.Entries![i].Binding;
                __descriptor.Entries[i].Visibility = (Silk.NET.WebGPU.ShaderStage)descriptor.Entries![i].Visibility;
                __descriptor.Entries[i].Buffer.Type = (Silk.NET.WebGPU.BufferBindingType)descriptor.Entries![i].Buffer.Type;
                __descriptor.Entries[i].Buffer.HasDynamicOffset = descriptor.Entries![i].Buffer.HasDynamicOffset;
                __descriptor.Entries[i].Buffer.MinBindingSize = descriptor.Entries![i].Buffer.MinBindingSize;
                __descriptor.Entries[i].Sampler.Type = (Silk.NET.WebGPU.SamplerBindingType)descriptor.Entries![i].Sampler.Type;
                __descriptor.Entries[i].Texture.SampleType = (Silk.NET.WebGPU.TextureSampleType)descriptor.Entries![i].Texture.SampleType;
                __descriptor.Entries[i].Texture.ViewDimension = (Silk.NET.WebGPU.TextureViewDimension)descriptor.Entries![i].Texture.ViewDimension;
                __descriptor.Entries[i].Texture.Multisampled = descriptor.Entries![i].Texture.Multisampled;
                __descriptor.Entries[i].StorageTexture.Access = (Silk.NET.WebGPU.StorageTextureAccess)descriptor.Entries![i].StorageTexture.Access;
                __descriptor.Entries[i].StorageTexture.Format = (Silk.NET.WebGPU.TextureFormat)descriptor.Entries![i].StorageTexture.Format;
                __descriptor.Entries[i].StorageTexture.ViewDimension = (Silk.NET.WebGPU.TextureViewDimension)descriptor.Entries![i].StorageTexture.ViewDimension;
            }
        }

        var result = WebGPU.DeviceCreateBindGroupLayout(Handle, in __descriptor);
        return new BindGroupLayout(WebGPU, result);
    }

    public unsafe Buffer CreateBuffer(in BufferDescriptor descriptor)
    {
        using var m = new MarshalHelper();
        Silk.NET.WebGPU.BufferDescriptor __descriptor = default;
        __descriptor.Label = m.RentUtf8Ptr(descriptor.Label);
        __descriptor.Usage = (Silk.NET.WebGPU.BufferUsage)descriptor.Usage;
        __descriptor.Size = descriptor.Size;
        __descriptor.MappedAtCreation = descriptor.MappedAtCreation;
        var result = WebGPU.DeviceCreateBuffer(Handle, in __descriptor);
        return new Buffer(WebGPU, result);
    }

    public unsafe CommandEncoder CreateCommandEncoder(string? label = null)
    {
        using var m = new MarshalHelper();
        Silk.NET.WebGPU.CommandEncoderDescriptor __descriptor = default;
        __descriptor.Label = m.RentUtf8Ptr(label);
        var result = WebGPU.DeviceCreateCommandEncoder(Handle, in __descriptor);
        return new CommandEncoder(WebGPU, result);
    }

    public unsafe ComputePipeline CreateComputePipeline(in ComputePipelineDescriptor descriptor)
    {
        using var m = new MarshalHelper();
        Silk.NET.WebGPU.ComputePipelineDescriptor __descriptor = default;
        __descriptor.Label = m.RentUtf8Ptr(descriptor.Label);
        __descriptor.Layout = descriptor.Layout;
        __descriptor.Compute.Module = descriptor.Compute.Module;
        __descriptor.Compute.EntryPoint = m.RentUtf8Ptr(descriptor.Compute.EntryPoint);
        if (descriptor.Compute.Constants != null)
        {
            __descriptor.Compute.ConstantCount = (nuint)descriptor.Compute.Constants!.Length;
            __descriptor.Compute.Constants = MarshalHelper.AsPointer(descriptor.Compute.Constants!.Length <= MarshalHelper.MaxStack ? stackalloc Silk.NET.WebGPU.ConstantEntry[descriptor.Compute.Constants!.Length] : m.RentSpan<Silk.NET.WebGPU.ConstantEntry>(descriptor.Compute.Constants!.Length));
            for (int i = 0; i < descriptor.Compute.Constants!.Length; i++)
            {
                __descriptor.Compute.Constants[i].Key = m.RentUtf8Ptr(descriptor.Compute.Constants![i].Key);
                __descriptor.Compute.Constants[i].Value = descriptor.Compute.Constants![i].Value;
            }
        }

        var result = WebGPU.DeviceCreateComputePipeline(Handle, in __descriptor);
        return new ComputePipeline(WebGPU, result);
    }

    public unsafe void CreateComputePipelineAsync(in ComputePipelineDescriptor descriptor, PfnCreateComputePipelineAsyncCallback callback)
    {
        using var m = new MarshalHelper();
        Silk.NET.WebGPU.ComputePipelineDescriptor __descriptor = default;
        __descriptor.Label = m.RentUtf8Ptr(descriptor.Label);
        __descriptor.Layout = descriptor.Layout;
        __descriptor.Compute.Module = descriptor.Compute.Module;
        __descriptor.Compute.EntryPoint = m.RentUtf8Ptr(descriptor.Compute.EntryPoint);
        if (descriptor.Compute.Constants != null)
        {
            __descriptor.Compute.ConstantCount = (nuint)descriptor.Compute.Constants!.Length;
            __descriptor.Compute.Constants = MarshalHelper.AsPointer(descriptor.Compute.Constants!.Length <= MarshalHelper.MaxStack ? stackalloc Silk.NET.WebGPU.ConstantEntry[descriptor.Compute.Constants!.Length] : m.RentSpan<Silk.NET.WebGPU.ConstantEntry>(descriptor.Compute.Constants!.Length));
            for (int i = 0; i < descriptor.Compute.Constants!.Length; i++)
            {
                __descriptor.Compute.Constants[i].Key = m.RentUtf8Ptr(descriptor.Compute.Constants![i].Key);
                __descriptor.Compute.Constants[i].Value = descriptor.Compute.Constants![i].Value;
            }
        }

        WebGPU.DeviceCreateComputePipelineAsync(Handle, in __descriptor, callback, null);
    }

    public unsafe void CreateComputePipelineAsync<T0>(in ComputePipelineDescriptor descriptor, PfnCreateComputePipelineAsyncCallback callback, ref T0 userdata)
        where T0 : unmanaged
    {
        using var m = new MarshalHelper();
        Silk.NET.WebGPU.ComputePipelineDescriptor __descriptor = default;
        __descriptor.Label = m.RentUtf8Ptr(descriptor.Label);
        __descriptor.Layout = descriptor.Layout;
        __descriptor.Compute.Module = descriptor.Compute.Module;
        __descriptor.Compute.EntryPoint = m.RentUtf8Ptr(descriptor.Compute.EntryPoint);
        if (descriptor.Compute.Constants != null)
        {
            __descriptor.Compute.ConstantCount = (nuint)descriptor.Compute.Constants!.Length;
            __descriptor.Compute.Constants = MarshalHelper.AsPointer(descriptor.Compute.Constants!.Length <= MarshalHelper.MaxStack ? stackalloc Silk.NET.WebGPU.ConstantEntry[descriptor.Compute.Constants!.Length] : m.RentSpan<Silk.NET.WebGPU.ConstantEntry>(descriptor.Compute.Constants!.Length));
            for (int i = 0; i < descriptor.Compute.Constants!.Length; i++)
            {
                __descriptor.Compute.Constants[i].Key = m.RentUtf8Ptr(descriptor.Compute.Constants![i].Key);
                __descriptor.Compute.Constants[i].Value = descriptor.Compute.Constants![i].Value;
            }
        }

        WebGPU.DeviceCreateComputePipelineAsync<T0>(Handle, in __descriptor, callback, ref userdata);
    }

    public unsafe PipelineLayout CreatePipelineLayout(in PipelineLayoutDescriptor descriptor)
    {
        using var m = new MarshalHelper();
        Silk.NET.WebGPU.PipelineLayoutDescriptor __descriptor = default;
        __descriptor.Label = m.RentUtf8Ptr(descriptor.Label);
        if (descriptor.BindGroupLayouts != null)
        {
            __descriptor.BindGroupLayoutCount = (nuint)descriptor.BindGroupLayouts!.Length;
            __descriptor.BindGroupLayouts = (Silk.NET.WebGPU.BindGroupLayout**)m.Pin<BindGroupLayoutHandle, BindGroupLayoutHandle>(descriptor.BindGroupLayouts);
        }

        var result = WebGPU.DeviceCreatePipelineLayout(Handle, in __descriptor);
        return new PipelineLayout(WebGPU, result);
    }

    public unsafe QuerySet CreateQuerySet(in QuerySetDescriptor descriptor)
    {
        using var m = new MarshalHelper();
        Silk.NET.WebGPU.QuerySetDescriptor __descriptor = default;
        __descriptor.Label = m.RentUtf8Ptr(descriptor.Label);
        __descriptor.Type = (Silk.NET.WebGPU.QueryType)descriptor.Type;
        __descriptor.Count = descriptor.Count;
        var result = WebGPU.DeviceCreateQuerySet(Handle, in __descriptor);
        return new QuerySet(WebGPU, result);
    }

    public unsafe RenderBundleEncoder CreateRenderBundleEncoder(in RenderBundleEncoderDescriptor descriptor)
    {
        using var m = new MarshalHelper();
        Silk.NET.WebGPU.RenderBundleEncoderDescriptor __descriptor = default;
        __descriptor.Label = m.RentUtf8Ptr(descriptor.Label);
        if (descriptor.ColorFormats != null)
        {
            __descriptor.ColorFormatCount = (nuint)descriptor.ColorFormats!.Length;
            __descriptor.ColorFormats = m.Pin<TextureFormat, Silk.NET.WebGPU.TextureFormat>(descriptor.ColorFormats);
        }

        __descriptor.DepthStencilFormat = (Silk.NET.WebGPU.TextureFormat)descriptor.DepthStencilFormat;
        __descriptor.SampleCount = descriptor.SampleCount;
        __descriptor.DepthReadOnly = descriptor.DepthReadOnly;
        __descriptor.StencilReadOnly = descriptor.StencilReadOnly;
        var result = WebGPU.DeviceCreateRenderBundleEncoder(Handle, in __descriptor);
        return new RenderBundleEncoder(WebGPU, result);
    }

    public unsafe RenderPipeline CreateRenderPipeline(in RenderPipelineDescriptor descriptor)
    {
        using var m = new MarshalHelper();
        Silk.NET.WebGPU.RenderPipelineDescriptor __descriptor = default;
        __descriptor.Label = m.RentUtf8Ptr(descriptor.Label);
        __descriptor.Layout = descriptor.Layout;
        __descriptor.Vertex.Module = descriptor.Vertex.Module;
        __descriptor.Vertex.EntryPoint = m.RentUtf8Ptr(descriptor.Vertex.EntryPoint);
        if (descriptor.Vertex.Constants != null)
        {
            __descriptor.Vertex.ConstantCount = (nuint)descriptor.Vertex.Constants!.Length;
            __descriptor.Vertex.Constants = MarshalHelper.AsPointer(descriptor.Vertex.Constants!.Length <= MarshalHelper.MaxStack ? stackalloc Silk.NET.WebGPU.ConstantEntry[descriptor.Vertex.Constants!.Length] : m.RentSpan<Silk.NET.WebGPU.ConstantEntry>(descriptor.Vertex.Constants!.Length));
            for (int i = 0; i < descriptor.Vertex.Constants!.Length; i++)
            {
                __descriptor.Vertex.Constants[i].Key = m.RentUtf8Ptr(descriptor.Vertex.Constants![i].Key);
                __descriptor.Vertex.Constants[i].Value = descriptor.Vertex.Constants![i].Value;
            }
        }

        if (descriptor.Vertex.Buffers != null)
        {
            __descriptor.Vertex.BufferCount = (nuint)descriptor.Vertex.Buffers!.Length;
            __descriptor.Vertex.Buffers = MarshalHelper.AsPointer(descriptor.Vertex.Buffers!.Length <= MarshalHelper.MaxStack ? stackalloc Silk.NET.WebGPU.VertexBufferLayout[descriptor.Vertex.Buffers!.Length] : m.RentSpan<Silk.NET.WebGPU.VertexBufferLayout>(descriptor.Vertex.Buffers!.Length));
            for (int i = 0; i < descriptor.Vertex.Buffers!.Length; i++)
            {
                __descriptor.Vertex.Buffers[i].ArrayStride = descriptor.Vertex.Buffers![i].ArrayStride;
                __descriptor.Vertex.Buffers[i].StepMode = (Silk.NET.WebGPU.VertexStepMode)descriptor.Vertex.Buffers![i].StepMode;
                if (descriptor.Vertex.Buffers![i].Attributes != null)
                {
                    __descriptor.Vertex.Buffers[i].AttributeCount = (nuint)descriptor.Vertex.Buffers![i].Attributes!.Length;
                    __descriptor.Vertex.Buffers[i].Attributes = m.Pin<VertexAttribute, Silk.NET.WebGPU.VertexAttribute>(descriptor.Vertex.Buffers![i].Attributes);
                }
            }
        }

        __descriptor.Primitive.Topology = (Silk.NET.WebGPU.PrimitiveTopology)descriptor.Primitive.Topology;
        __descriptor.Primitive.StripIndexFormat = (Silk.NET.WebGPU.IndexFormat)descriptor.Primitive.StripIndexFormat;
        __descriptor.Primitive.FrontFace = (Silk.NET.WebGPU.FrontFace)descriptor.Primitive.FrontFace;
        __descriptor.Primitive.CullMode = (Silk.NET.WebGPU.CullMode)descriptor.Primitive.CullMode;
        Silk.NET.WebGPU.DepthStencilState __descriptor_DepthStencil = default;
        if (descriptor.DepthStencil.HasValue)
        {
            __descriptor_DepthStencil.Format = (Silk.NET.WebGPU.TextureFormat)descriptor.DepthStencil!.Value.Format;
            __descriptor_DepthStencil.DepthWriteEnabled = descriptor.DepthStencil!.Value.DepthWriteEnabled;
            __descriptor_DepthStencil.DepthCompare = (Silk.NET.WebGPU.CompareFunction)descriptor.DepthStencil!.Value.DepthCompare;
            __descriptor_DepthStencil.StencilFront = Unsafe.BitCast<StencilFaceState, Silk.NET.WebGPU.StencilFaceState>(descriptor.DepthStencil!.Value.StencilFront);
            __descriptor_DepthStencil.StencilBack = Unsafe.BitCast<StencilFaceState, Silk.NET.WebGPU.StencilFaceState>(descriptor.DepthStencil!.Value.StencilBack);
            __descriptor_DepthStencil.StencilReadMask = descriptor.DepthStencil!.Value.StencilReadMask;
            __descriptor_DepthStencil.StencilWriteMask = descriptor.DepthStencil!.Value.StencilWriteMask;
            __descriptor_DepthStencil.DepthBias = descriptor.DepthStencil!.Value.DepthBias;
            __descriptor_DepthStencil.DepthBiasSlopeScale = descriptor.DepthStencil!.Value.DepthBiasSlopeScale;
            __descriptor_DepthStencil.DepthBiasClamp = descriptor.DepthStencil!.Value.DepthBiasClamp;
            __descriptor.DepthStencil = &__descriptor_DepthStencil; //PINREF
        }

        __descriptor.Multisample.Count = descriptor.Multisample.Count;
        __descriptor.Multisample.Mask = descriptor.Multisample.Mask;
        __descriptor.Multisample.AlphaToCoverageEnabled = descriptor.Multisample.AlphaToCoverageEnabled;
        Silk.NET.WebGPU.FragmentState __descriptor_Fragment = default;
        if (descriptor.Fragment.HasValue)
        {
            __descriptor_Fragment.Module = descriptor.Fragment!.Value.Module;
            __descriptor_Fragment.EntryPoint = m.RentUtf8Ptr(descriptor.Fragment!.Value.EntryPoint);
            if (descriptor.Fragment!.Value.Constants != null)
            {
                __descriptor_Fragment.ConstantCount = (nuint)descriptor.Fragment!.Value.Constants!.Length;
                __descriptor_Fragment.Constants = MarshalHelper.AsPointer(descriptor.Fragment!.Value.Constants!.Length <= MarshalHelper.MaxStack ? stackalloc Silk.NET.WebGPU.ConstantEntry[descriptor.Fragment!.Value.Constants!.Length] : m.RentSpan<Silk.NET.WebGPU.ConstantEntry>(descriptor.Fragment!.Value.Constants!.Length));
                for (int i = 0; i < descriptor.Fragment!.Value.Constants!.Length; i++)
                {
                    __descriptor_Fragment.Constants[i].Key = m.RentUtf8Ptr(descriptor.Fragment!.Value.Constants![i].Key);
                    __descriptor_Fragment.Constants[i].Value = descriptor.Fragment!.Value.Constants![i].Value;
                }
            }

            if (descriptor.Fragment!.Value.Targets != null)
            {
                __descriptor_Fragment.TargetCount = (nuint)descriptor.Fragment!.Value.Targets!.Length;
                __descriptor_Fragment.Targets = MarshalHelper.AsPointer(descriptor.Fragment!.Value.Targets!.Length <= MarshalHelper.MaxStack ? stackalloc Silk.NET.WebGPU.ColorTargetState[descriptor.Fragment!.Value.Targets!.Length] : m.RentSpan<Silk.NET.WebGPU.ColorTargetState>(descriptor.Fragment!.Value.Targets!.Length));
                for (int i = 0; i < descriptor.Fragment!.Value.Targets!.Length; i++)
                {
                    __descriptor_Fragment.Targets[i].Format = (Silk.NET.WebGPU.TextureFormat)descriptor.Fragment!.Value.Targets![i].Format;
                    Silk.NET.WebGPU.BlendState __descriptor_Fragment_Targets_i__Blend = default;
                    if (descriptor.Fragment!.Value.Targets![i].Blend.HasValue)
                    {
                        __descriptor_Fragment_Targets_i__Blend = Unsafe.BitCast<BlendState, Silk.NET.WebGPU.BlendState>(descriptor.Fragment!.Value.Targets![i].Blend!.Value);
                        __descriptor_Fragment.Targets[i].Blend = m.RentPtr(ref __descriptor_Fragment_Targets_i__Blend); //PINREF
                    }

                    __descriptor_Fragment.Targets[i].WriteMask = (Silk.NET.WebGPU.ColorWriteMask)descriptor.Fragment!.Value.Targets![i].WriteMask;
                }
            }

            __descriptor.Fragment = &__descriptor_Fragment; //PINREF
        }

        var result = WebGPU.DeviceCreateRenderPipeline(Handle, in __descriptor);
        return new RenderPipeline(WebGPU, result);
    }

    public unsafe void CreateRenderPipelineAsync(in RenderPipelineDescriptor descriptor, PfnCreateRenderPipelineAsyncCallback callback)
    {
        using var m = new MarshalHelper();
        Silk.NET.WebGPU.RenderPipelineDescriptor __descriptor = default;
        __descriptor.Label = m.RentUtf8Ptr(descriptor.Label);
        __descriptor.Layout = descriptor.Layout;
        __descriptor.Vertex.Module = descriptor.Vertex.Module;
        __descriptor.Vertex.EntryPoint = m.RentUtf8Ptr(descriptor.Vertex.EntryPoint);
        if (descriptor.Vertex.Constants != null)
        {
            __descriptor.Vertex.ConstantCount = (nuint)descriptor.Vertex.Constants!.Length;
            __descriptor.Vertex.Constants = MarshalHelper.AsPointer(descriptor.Vertex.Constants!.Length <= MarshalHelper.MaxStack ? stackalloc Silk.NET.WebGPU.ConstantEntry[descriptor.Vertex.Constants!.Length] : m.RentSpan<Silk.NET.WebGPU.ConstantEntry>(descriptor.Vertex.Constants!.Length));
            for (int i = 0; i < descriptor.Vertex.Constants!.Length; i++)
            {
                __descriptor.Vertex.Constants[i].Key = m.RentUtf8Ptr(descriptor.Vertex.Constants![i].Key);
                __descriptor.Vertex.Constants[i].Value = descriptor.Vertex.Constants![i].Value;
            }
        }

        if (descriptor.Vertex.Buffers != null)
        {
            __descriptor.Vertex.BufferCount = (nuint)descriptor.Vertex.Buffers!.Length;
            __descriptor.Vertex.Buffers = MarshalHelper.AsPointer(descriptor.Vertex.Buffers!.Length <= MarshalHelper.MaxStack ? stackalloc Silk.NET.WebGPU.VertexBufferLayout[descriptor.Vertex.Buffers!.Length] : m.RentSpan<Silk.NET.WebGPU.VertexBufferLayout>(descriptor.Vertex.Buffers!.Length));
            for (int i = 0; i < descriptor.Vertex.Buffers!.Length; i++)
            {
                __descriptor.Vertex.Buffers[i].ArrayStride = descriptor.Vertex.Buffers![i].ArrayStride;
                __descriptor.Vertex.Buffers[i].StepMode = (Silk.NET.WebGPU.VertexStepMode)descriptor.Vertex.Buffers![i].StepMode;
                if (descriptor.Vertex.Buffers![i].Attributes != null)
                {
                    __descriptor.Vertex.Buffers[i].AttributeCount = (nuint)descriptor.Vertex.Buffers![i].Attributes!.Length;
                    __descriptor.Vertex.Buffers[i].Attributes = m.Pin<VertexAttribute, Silk.NET.WebGPU.VertexAttribute>(descriptor.Vertex.Buffers![i].Attributes);
                }
            }
        }

        __descriptor.Primitive.Topology = (Silk.NET.WebGPU.PrimitiveTopology)descriptor.Primitive.Topology;
        __descriptor.Primitive.StripIndexFormat = (Silk.NET.WebGPU.IndexFormat)descriptor.Primitive.StripIndexFormat;
        __descriptor.Primitive.FrontFace = (Silk.NET.WebGPU.FrontFace)descriptor.Primitive.FrontFace;
        __descriptor.Primitive.CullMode = (Silk.NET.WebGPU.CullMode)descriptor.Primitive.CullMode;
        Silk.NET.WebGPU.DepthStencilState __descriptor_DepthStencil = default;
        if (descriptor.DepthStencil.HasValue)
        {
            __descriptor_DepthStencil.Format = (Silk.NET.WebGPU.TextureFormat)descriptor.DepthStencil!.Value.Format;
            __descriptor_DepthStencil.DepthWriteEnabled = descriptor.DepthStencil!.Value.DepthWriteEnabled;
            __descriptor_DepthStencil.DepthCompare = (Silk.NET.WebGPU.CompareFunction)descriptor.DepthStencil!.Value.DepthCompare;
            __descriptor_DepthStencil.StencilFront = Unsafe.BitCast<StencilFaceState, Silk.NET.WebGPU.StencilFaceState>(descriptor.DepthStencil!.Value.StencilFront);
            __descriptor_DepthStencil.StencilBack = Unsafe.BitCast<StencilFaceState, Silk.NET.WebGPU.StencilFaceState>(descriptor.DepthStencil!.Value.StencilBack);
            __descriptor_DepthStencil.StencilReadMask = descriptor.DepthStencil!.Value.StencilReadMask;
            __descriptor_DepthStencil.StencilWriteMask = descriptor.DepthStencil!.Value.StencilWriteMask;
            __descriptor_DepthStencil.DepthBias = descriptor.DepthStencil!.Value.DepthBias;
            __descriptor_DepthStencil.DepthBiasSlopeScale = descriptor.DepthStencil!.Value.DepthBiasSlopeScale;
            __descriptor_DepthStencil.DepthBiasClamp = descriptor.DepthStencil!.Value.DepthBiasClamp;
            __descriptor.DepthStencil = &__descriptor_DepthStencil; //PINREF
        }

        __descriptor.Multisample.Count = descriptor.Multisample.Count;
        __descriptor.Multisample.Mask = descriptor.Multisample.Mask;
        __descriptor.Multisample.AlphaToCoverageEnabled = descriptor.Multisample.AlphaToCoverageEnabled;
        Silk.NET.WebGPU.FragmentState __descriptor_Fragment = default;
        if (descriptor.Fragment.HasValue)
        {
            __descriptor_Fragment.Module = descriptor.Fragment!.Value.Module;
            __descriptor_Fragment.EntryPoint = m.RentUtf8Ptr(descriptor.Fragment!.Value.EntryPoint);
            if (descriptor.Fragment!.Value.Constants != null)
            {
                __descriptor_Fragment.ConstantCount = (nuint)descriptor.Fragment!.Value.Constants!.Length;
                __descriptor_Fragment.Constants = MarshalHelper.AsPointer(descriptor.Fragment!.Value.Constants!.Length <= MarshalHelper.MaxStack ? stackalloc Silk.NET.WebGPU.ConstantEntry[descriptor.Fragment!.Value.Constants!.Length] : m.RentSpan<Silk.NET.WebGPU.ConstantEntry>(descriptor.Fragment!.Value.Constants!.Length));
                for (int i = 0; i < descriptor.Fragment!.Value.Constants!.Length; i++)
                {
                    __descriptor_Fragment.Constants[i].Key = m.RentUtf8Ptr(descriptor.Fragment!.Value.Constants![i].Key);
                    __descriptor_Fragment.Constants[i].Value = descriptor.Fragment!.Value.Constants![i].Value;
                }
            }

            if (descriptor.Fragment!.Value.Targets != null)
            {
                __descriptor_Fragment.TargetCount = (nuint)descriptor.Fragment!.Value.Targets!.Length;
                __descriptor_Fragment.Targets = MarshalHelper.AsPointer(descriptor.Fragment!.Value.Targets!.Length <= MarshalHelper.MaxStack ? stackalloc Silk.NET.WebGPU.ColorTargetState[descriptor.Fragment!.Value.Targets!.Length] : m.RentSpan<Silk.NET.WebGPU.ColorTargetState>(descriptor.Fragment!.Value.Targets!.Length));
                for (int i = 0; i < descriptor.Fragment!.Value.Targets!.Length; i++)
                {
                    __descriptor_Fragment.Targets[i].Format = (Silk.NET.WebGPU.TextureFormat)descriptor.Fragment!.Value.Targets![i].Format;
                    Silk.NET.WebGPU.BlendState __descriptor_Fragment_Targets_i__Blend = default;
                    if (descriptor.Fragment!.Value.Targets![i].Blend.HasValue)
                    {
                        __descriptor_Fragment_Targets_i__Blend = Unsafe.BitCast<BlendState, Silk.NET.WebGPU.BlendState>(descriptor.Fragment!.Value.Targets![i].Blend!.Value);
                        __descriptor_Fragment.Targets[i].Blend = m.RentPtr(ref __descriptor_Fragment_Targets_i__Blend); //PINREF
                    }

                    __descriptor_Fragment.Targets[i].WriteMask = (Silk.NET.WebGPU.ColorWriteMask)descriptor.Fragment!.Value.Targets![i].WriteMask;
                }
            }

            __descriptor.Fragment = &__descriptor_Fragment; //PINREF
        }

        WebGPU.DeviceCreateRenderPipelineAsync(Handle, in __descriptor, callback, null);
    }

    public unsafe void CreateRenderPipelineAsync<T0>(in RenderPipelineDescriptor descriptor, PfnCreateRenderPipelineAsyncCallback callback, ref T0 userdata)
        where T0 : unmanaged
    {
        using var m = new MarshalHelper();
        Silk.NET.WebGPU.RenderPipelineDescriptor __descriptor = default;
        __descriptor.Label = m.RentUtf8Ptr(descriptor.Label);
        __descriptor.Layout = descriptor.Layout;
        __descriptor.Vertex.Module = descriptor.Vertex.Module;
        __descriptor.Vertex.EntryPoint = m.RentUtf8Ptr(descriptor.Vertex.EntryPoint);
        if (descriptor.Vertex.Constants != null)
        {
            __descriptor.Vertex.ConstantCount = (nuint)descriptor.Vertex.Constants!.Length;
            __descriptor.Vertex.Constants = MarshalHelper.AsPointer(descriptor.Vertex.Constants!.Length <= MarshalHelper.MaxStack ? stackalloc Silk.NET.WebGPU.ConstantEntry[descriptor.Vertex.Constants!.Length] : m.RentSpan<Silk.NET.WebGPU.ConstantEntry>(descriptor.Vertex.Constants!.Length));
            for (int i = 0; i < descriptor.Vertex.Constants!.Length; i++)
            {
                __descriptor.Vertex.Constants[i].Key = m.RentUtf8Ptr(descriptor.Vertex.Constants![i].Key);
                __descriptor.Vertex.Constants[i].Value = descriptor.Vertex.Constants![i].Value;
            }
        }

        if (descriptor.Vertex.Buffers != null)
        {
            __descriptor.Vertex.BufferCount = (nuint)descriptor.Vertex.Buffers!.Length;
            __descriptor.Vertex.Buffers = MarshalHelper.AsPointer(descriptor.Vertex.Buffers!.Length <= MarshalHelper.MaxStack ? stackalloc Silk.NET.WebGPU.VertexBufferLayout[descriptor.Vertex.Buffers!.Length] : m.RentSpan<Silk.NET.WebGPU.VertexBufferLayout>(descriptor.Vertex.Buffers!.Length));
            for (int i = 0; i < descriptor.Vertex.Buffers!.Length; i++)
            {
                __descriptor.Vertex.Buffers[i].ArrayStride = descriptor.Vertex.Buffers![i].ArrayStride;
                __descriptor.Vertex.Buffers[i].StepMode = (Silk.NET.WebGPU.VertexStepMode)descriptor.Vertex.Buffers![i].StepMode;
                if (descriptor.Vertex.Buffers![i].Attributes != null)
                {
                    __descriptor.Vertex.Buffers[i].AttributeCount = (nuint)descriptor.Vertex.Buffers![i].Attributes!.Length;
                    __descriptor.Vertex.Buffers[i].Attributes = m.Pin<VertexAttribute, Silk.NET.WebGPU.VertexAttribute>(descriptor.Vertex.Buffers![i].Attributes);
                }
            }
        }

        __descriptor.Primitive.Topology = (Silk.NET.WebGPU.PrimitiveTopology)descriptor.Primitive.Topology;
        __descriptor.Primitive.StripIndexFormat = (Silk.NET.WebGPU.IndexFormat)descriptor.Primitive.StripIndexFormat;
        __descriptor.Primitive.FrontFace = (Silk.NET.WebGPU.FrontFace)descriptor.Primitive.FrontFace;
        __descriptor.Primitive.CullMode = (Silk.NET.WebGPU.CullMode)descriptor.Primitive.CullMode;
        Silk.NET.WebGPU.DepthStencilState __descriptor_DepthStencil = default;
        if (descriptor.DepthStencil.HasValue)
        {
            __descriptor_DepthStencil.Format = (Silk.NET.WebGPU.TextureFormat)descriptor.DepthStencil!.Value.Format;
            __descriptor_DepthStencil.DepthWriteEnabled = descriptor.DepthStencil!.Value.DepthWriteEnabled;
            __descriptor_DepthStencil.DepthCompare = (Silk.NET.WebGPU.CompareFunction)descriptor.DepthStencil!.Value.DepthCompare;
            __descriptor_DepthStencil.StencilFront = Unsafe.BitCast<StencilFaceState, Silk.NET.WebGPU.StencilFaceState>(descriptor.DepthStencil!.Value.StencilFront);
            __descriptor_DepthStencil.StencilBack = Unsafe.BitCast<StencilFaceState, Silk.NET.WebGPU.StencilFaceState>(descriptor.DepthStencil!.Value.StencilBack);
            __descriptor_DepthStencil.StencilReadMask = descriptor.DepthStencil!.Value.StencilReadMask;
            __descriptor_DepthStencil.StencilWriteMask = descriptor.DepthStencil!.Value.StencilWriteMask;
            __descriptor_DepthStencil.DepthBias = descriptor.DepthStencil!.Value.DepthBias;
            __descriptor_DepthStencil.DepthBiasSlopeScale = descriptor.DepthStencil!.Value.DepthBiasSlopeScale;
            __descriptor_DepthStencil.DepthBiasClamp = descriptor.DepthStencil!.Value.DepthBiasClamp;
            __descriptor.DepthStencil = &__descriptor_DepthStencil; //PINREF
        }

        __descriptor.Multisample.Count = descriptor.Multisample.Count;
        __descriptor.Multisample.Mask = descriptor.Multisample.Mask;
        __descriptor.Multisample.AlphaToCoverageEnabled = descriptor.Multisample.AlphaToCoverageEnabled;
        Silk.NET.WebGPU.FragmentState __descriptor_Fragment = default;
        if (descriptor.Fragment.HasValue)
        {
            __descriptor_Fragment.Module = descriptor.Fragment!.Value.Module;
            __descriptor_Fragment.EntryPoint = m.RentUtf8Ptr(descriptor.Fragment!.Value.EntryPoint);
            if (descriptor.Fragment!.Value.Constants != null)
            {
                __descriptor_Fragment.ConstantCount = (nuint)descriptor.Fragment!.Value.Constants!.Length;
                __descriptor_Fragment.Constants = MarshalHelper.AsPointer(descriptor.Fragment!.Value.Constants!.Length <= MarshalHelper.MaxStack ? stackalloc Silk.NET.WebGPU.ConstantEntry[descriptor.Fragment!.Value.Constants!.Length] : m.RentSpan<Silk.NET.WebGPU.ConstantEntry>(descriptor.Fragment!.Value.Constants!.Length));
                for (int i = 0; i < descriptor.Fragment!.Value.Constants!.Length; i++)
                {
                    __descriptor_Fragment.Constants[i].Key = m.RentUtf8Ptr(descriptor.Fragment!.Value.Constants![i].Key);
                    __descriptor_Fragment.Constants[i].Value = descriptor.Fragment!.Value.Constants![i].Value;
                }
            }

            if (descriptor.Fragment!.Value.Targets != null)
            {
                __descriptor_Fragment.TargetCount = (nuint)descriptor.Fragment!.Value.Targets!.Length;
                __descriptor_Fragment.Targets = MarshalHelper.AsPointer(descriptor.Fragment!.Value.Targets!.Length <= MarshalHelper.MaxStack ? stackalloc Silk.NET.WebGPU.ColorTargetState[descriptor.Fragment!.Value.Targets!.Length] : m.RentSpan<Silk.NET.WebGPU.ColorTargetState>(descriptor.Fragment!.Value.Targets!.Length));
                for (int i = 0; i < descriptor.Fragment!.Value.Targets!.Length; i++)
                {
                    __descriptor_Fragment.Targets[i].Format = (Silk.NET.WebGPU.TextureFormat)descriptor.Fragment!.Value.Targets![i].Format;
                    Silk.NET.WebGPU.BlendState __descriptor_Fragment_Targets_i__Blend = default;
                    if (descriptor.Fragment!.Value.Targets![i].Blend.HasValue)
                    {
                        __descriptor_Fragment_Targets_i__Blend = Unsafe.BitCast<BlendState, Silk.NET.WebGPU.BlendState>(descriptor.Fragment!.Value.Targets![i].Blend!.Value);
                        __descriptor_Fragment.Targets[i].Blend = m.RentPtr(ref __descriptor_Fragment_Targets_i__Blend); //PINREF
                    }

                    __descriptor_Fragment.Targets[i].WriteMask = (Silk.NET.WebGPU.ColorWriteMask)descriptor.Fragment!.Value.Targets![i].WriteMask;
                }
            }

            __descriptor.Fragment = &__descriptor_Fragment; //PINREF
        }

        WebGPU.DeviceCreateRenderPipelineAsync<T0>(Handle, in __descriptor, callback, ref userdata);
    }

    public unsafe Sampler CreateSampler(in SamplerDescriptor descriptor)
    {
        using var m = new MarshalHelper();
        Silk.NET.WebGPU.SamplerDescriptor __descriptor = default;
        __descriptor.Label = m.RentUtf8Ptr(descriptor.Label);
        __descriptor.AddressModeU = (Silk.NET.WebGPU.AddressMode)descriptor.AddressModeU;
        __descriptor.AddressModeV = (Silk.NET.WebGPU.AddressMode)descriptor.AddressModeV;
        __descriptor.AddressModeW = (Silk.NET.WebGPU.AddressMode)descriptor.AddressModeW;
        __descriptor.MagFilter = (Silk.NET.WebGPU.FilterMode)descriptor.MagFilter;
        __descriptor.MinFilter = (Silk.NET.WebGPU.FilterMode)descriptor.MinFilter;
        __descriptor.MipmapFilter = (Silk.NET.WebGPU.MipmapFilterMode)descriptor.MipmapFilter;
        __descriptor.LodMinClamp = descriptor.LodMinClamp;
        __descriptor.LodMaxClamp = descriptor.LodMaxClamp;
        __descriptor.Compare = (Silk.NET.WebGPU.CompareFunction)descriptor.Compare;
        __descriptor.MaxAnisotropy = descriptor.MaxAnisotropy;
        var result = WebGPU.DeviceCreateSampler(Handle, in __descriptor);
        return new Sampler(WebGPU, result);
    }

    public unsafe Texture CreateTexture(in TextureDescriptor descriptor)
    {
        using var m = new MarshalHelper();
        Silk.NET.WebGPU.TextureDescriptor __descriptor = default;
        __descriptor.Label = m.RentUtf8Ptr(descriptor.Label);
        __descriptor.Usage = (Silk.NET.WebGPU.TextureUsage)descriptor.Usage;
        __descriptor.Dimension = (Silk.NET.WebGPU.TextureDimension)descriptor.Dimension;
        __descriptor.Size = Unsafe.BitCast<Extent3D, Silk.NET.WebGPU.Extent3D>(descriptor.Size);
        __descriptor.Format = (Silk.NET.WebGPU.TextureFormat)descriptor.Format;
        __descriptor.MipLevelCount = descriptor.MipLevelCount;
        __descriptor.SampleCount = descriptor.SampleCount;
        if (descriptor.ViewFormats != null)
        {
            __descriptor.ViewFormatCount = (nuint)descriptor.ViewFormats!.Length;
            __descriptor.ViewFormats = m.Pin<TextureFormat, Silk.NET.WebGPU.TextureFormat>(descriptor.ViewFormats);
        }

        var result = WebGPU.DeviceCreateTexture(Handle, in __descriptor);
        return new Texture(WebGPU, result);
    }

    public unsafe void Destroy()
    {
        WebGPU.DeviceDestroy(Handle);
    }

    public unsafe nuint EnumerateFeatures(ref FeatureName features)
    {
        var result = WebGPU.DeviceEnumerateFeatures(Handle, ref Unsafe.As<FeatureName, Silk.NET.WebGPU.FeatureName>(ref features));
        return result;
    }

    public unsafe Bool32 GetLimits(ref SupportedLimits limits)
    {
        Silk.NET.WebGPU.SupportedLimits __limits = default;
        var result = WebGPU.DeviceGetLimits(Handle, ref __limits);
        limits.Limits = Unsafe.BitCast<Silk.NET.WebGPU.Limits, Limits>(__limits.Limits);
        return result;
    }

    public unsafe Queue GetQueue()
    {
        var result = WebGPU.DeviceGetQueue(Handle);
        return new Queue(WebGPU, result);
    }

    public unsafe Bool32 HasFeature(FeatureName feature)
    {
        var result = WebGPU.DeviceHasFeature(Handle, (Silk.NET.WebGPU.FeatureName)feature);
        return result;
    }

    public unsafe void PopErrorScope(PfnErrorCallback callback)
    {
        WebGPU.DevicePopErrorScope(Handle, callback, null);
    }

    public unsafe void PopErrorScope<T0>(PfnErrorCallback callback, ref T0 userdata)
        where T0 : unmanaged
    {
        WebGPU.DevicePopErrorScope<T0>(Handle, callback, ref userdata);
    }

    public unsafe void PushErrorScope(ErrorFilter filter)
    {
        WebGPU.DevicePushErrorScope(Handle, (Silk.NET.WebGPU.ErrorFilter)filter);
    }

    public unsafe void SetLabel(string label)
    {
        using var m = new MarshalHelper();
        WebGPU.DeviceSetLabel(Handle, m.RentUtf8Ptr(label));
    }

    public unsafe void SetUncapturedErrorCallback(PfnErrorCallback callback)
    {
        WebGPU.DeviceSetUncapturedErrorCallback(Handle, callback, null);
    }

    public unsafe void SetUncapturedErrorCallback<T0>(PfnErrorCallback callback, ref T0 userdata)
        where T0 : unmanaged
    {
        WebGPU.DeviceSetUncapturedErrorCallback<T0>(Handle, callback, ref userdata);
    }

    public unsafe void Reference()
    {
        WebGPU.DeviceReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.DeviceRelease(Handle);
    }

    public void Dispose()
    {
        if (Handle.IsEmpty)
            return;
        Disposing();
        Destroy();
        Handle = default;
        Disposed();
    }

    partial void Disposing();
    partial void Disposed();
}