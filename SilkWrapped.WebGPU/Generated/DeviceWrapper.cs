namespace SilkWrapped.WebGPU;
using WebGPU = Silk.NET.WebGPU.WebGPU;
using Device = Silk.NET.WebGPU.Device;

public unsafe partial class DeviceWrapper
{
    public DeviceWrapper(WebGPU webGPU, WebGPUDisposal disposal, Device* rawPointer)
    {
        weakWebGPUReference = new WeakReference<WebGPU>(webGPU ?? throw new ArgumentNullException(nameof(webGPU)));
        weakWebGPUDisposalReference = new WeakReference<WebGPUDisposal>(disposal ?? throw new ArgumentNullException(nameof(disposal)));
        this.RawPointer = rawPointer == null ? rawPointer : throw new ArgumentNullException(nameof(rawPointer));
    }

    WeakReference<WebGPU> weakWebGPUReference;
    [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
    public WebGPU? WebGPU => weakWebGPUReference.TryGetTarget(out var target) ? target : null;
    WeakReference<WebGPUDisposal> weakWebGPUDisposalReference;
    [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
    public WebGPUDisposal? Disposal => weakWebGPUDisposalReference.TryGetTarget(out var target) ? target : null;
    [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
    public Device* RawPointer { get; }

    public PipelineLayoutWrapper CreatePipelineLayout(in PipelineLayoutDescriptor descriptor)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return new PipelineLayoutWrapper(WebGPU, Disposal, webGPU.DeviceCreatePipelineLayout(RawPointer, descriptor));
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public QuerySetWrapper CreateQuerySet(in QuerySetDescriptor descriptor)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return new QuerySetWrapper(WebGPU, Disposal, webGPU.DeviceCreateQuerySet(RawPointer, descriptor));
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public RenderBundleEncoderWrapper CreateRenderBundleEncoder(in RenderBundleEncoderDescriptor descriptor)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return new RenderBundleEncoderWrapper(WebGPU, Disposal, webGPU.DeviceCreateRenderBundleEncoder(RawPointer, descriptor));
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public RenderPipelineWrapper CreateRenderPipeline(in RenderPipelineDescriptor descriptor)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return new RenderPipelineWrapper(WebGPU, Disposal, webGPU.DeviceCreateRenderPipeline(RawPointer, descriptor));
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void CreateRenderPipelineAsync<T0>(in RenderPipelineDescriptor descriptor, PfnCreateRenderPipelineAsyncCallback callback, ref T0 userdata)
        where T0 : unmanaged
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.DeviceCreateRenderPipelineAsync(RawPointer, descriptor, callback, ref userdata);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public SamplerWrapper CreateSampler(in SamplerDescriptor descriptor)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return new SamplerWrapper(WebGPU, Disposal, webGPU.DeviceCreateSampler(RawPointer, descriptor));
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public ShaderModuleWrapper CreateShaderModule(in ShaderModuleDescriptor descriptor)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return new ShaderModuleWrapper(WebGPU, Disposal, webGPU.DeviceCreateShaderModule(RawPointer, descriptor));
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public TextureWrapper CreateTexture(in TextureDescriptor descriptor)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return new TextureWrapper(WebGPU, Disposal, webGPU.DeviceCreateTexture(RawPointer, descriptor));
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void Destroy()
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.DeviceDestroy(RawPointer);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public UIntPtr EnumerateFeatures(ref FeatureName features)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return webGPU.DeviceEnumerateFeatures(RawPointer, ref features);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public Bool32 GetLimits(ref SupportedLimits limits)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return webGPU.DeviceGetLimits(RawPointer, ref limits);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public QueueWrapper GetQueue()
    {
        if (WebGPU is WebGPU webGPU)
        {
            return new QueueWrapper(WebGPU, Disposal, webGPU.DeviceGetQueue(RawPointer));
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public Bool32 HasFeature(FeatureName feature)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return webGPU.DeviceHasFeature(RawPointer, feature);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public Bool32 PopErrorScope<T0>(PfnErrorCallback callback, ref T0 userdata)
        where T0 : unmanaged
    {
        if (WebGPU is WebGPU webGPU)
        {
            return webGPU.DevicePopErrorScope(RawPointer, callback, ref userdata);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void PushErrorScope(ErrorFilter filter)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.DevicePushErrorScope(RawPointer, filter);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void SetLostCallback<T0>(PfnDeviceLostCallback callback, ref T0 userdata)
        where T0 : unmanaged
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.DeviceSetDeviceLostCallback(RawPointer, callback, ref userdata);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void SetLabel(String label)
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.DeviceSetLabel(RawPointer, label);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void SetUncapturedErrorCallback<T0>(PfnErrorCallback callback, ref T0 userdata)
        where T0 : unmanaged
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.DeviceSetUncapturedErrorCallback(RawPointer, callback, ref userdata);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public Boolean TryGetExtension<T>(out T ext)
        where T : NativeExtension<WebGPU>
    {
        if (WebGPU is WebGPU webGPU)
        {
            return webGPU.TryGetDeviceExtension(RawPointer, out ext);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public Boolean IsExtensionPresent(String extension)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return webGPU.IsDeviceExtensionPresent(RawPointer, extension);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public PfnProc GetProcAddress(String procName)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return webGPU.GetProcAddress(RawPointer, procName);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public BindGroupWrapper CreateBindGroup(in BindGroupDescriptor descriptor)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return new BindGroupWrapper(WebGPU, Disposal, webGPU.DeviceCreateBindGroup(RawPointer, descriptor));
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public BindGroupLayoutWrapper CreateBindGroupLayout(in BindGroupLayoutDescriptor descriptor)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return new BindGroupLayoutWrapper(WebGPU, Disposal, webGPU.DeviceCreateBindGroupLayout(RawPointer, descriptor));
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public BufferWrapper CreateBuffer(in BufferDescriptor descriptor)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return new BufferWrapper(WebGPU, Disposal, webGPU.DeviceCreateBuffer(RawPointer, descriptor));
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public CommandEncoderWrapper CreateCommandEncoder(in CommandEncoderDescriptor descriptor)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return new CommandEncoderWrapper(WebGPU, Disposal, webGPU.DeviceCreateCommandEncoder(RawPointer, descriptor));
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public ComputePipelineWrapper CreateComputePipeline(in ComputePipelineDescriptor descriptor)
    {
        if (WebGPU is WebGPU webGPU)
        {
            return new ComputePipelineWrapper(WebGPU, Disposal, webGPU.DeviceCreateComputePipeline(RawPointer, descriptor));
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public void CreateComputePipelineAsync<T0>(in ComputePipelineDescriptor descriptor, PfnCreateComputePipelineAsyncCallback callback, ref T0 userdata)
        where T0 : unmanaged
    {
        if (WebGPU is WebGPU webGPU)
        {
            webGPU.DeviceCreateComputePipelineAsync(RawPointer, descriptor, callback, ref userdata);
        }
        else
        {
            throw new InvalidOperationException("WebGPU is null");
        }
    }

    public static implicit operator Device*(DeviceWrapper wrapper) => wrapper.RawPointer;
}