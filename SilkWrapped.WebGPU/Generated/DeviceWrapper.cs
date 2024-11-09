namespace SilkWrapped.WebGPU
{
    public unsafe partial class DeviceWrapper : System.IDisposable
    {
        public ApiContainer Api { get; }
        public Silk.NET.WebGPU.Device* Handle { get; }

        public DeviceWrapper(ApiContainer api, Silk.NET.WebGPU.Device* handle)
        {
            Api = api;
            Handle = handle;
        }

        public bool TryGetExtension<T>(out T ext)
            where T : Silk.NET.Core.Native.NativeExtension<Silk.NET.WebGPU.WebGPU>
        {
            var result = Api.Core.TryGetDeviceExtension(Handle, out ext);
            return result;
        }

        public bool IsExtensionPresent(string extension)
        {
            var result = Api.Core.IsDeviceExtensionPresent(Handle, extension);
            return result;
        }

        public Silk.NET.WebGPU.PfnProc GetProcAddress(ref readonly byte procName)
        {
            var result = Api.Core.GetProcAddress(Handle, in procName);
            return result;
        }

        public Silk.NET.WebGPU.PfnProc GetProcAddress(string procName)
        {
            var result = Api.Core.GetProcAddress(Handle, procName);
            return result;
        }

        partial void BindGroupWrapperCreated(BindGroupWrapper value);
        public BindGroupWrapper CreateBindGroup(ref readonly Silk.NET.WebGPU.BindGroupDescriptor descriptor)
        {
            var result = Api.Core.DeviceCreateBindGroup(Handle, in descriptor);
            if (result == null)
                return null;
            var resultWrapper = new BindGroupWrapper(Api, result);
            BindGroupWrapperCreated(resultWrapper);
            return resultWrapper;
        }

        partial void BindGroupLayoutWrapperCreated(BindGroupLayoutWrapper value);
        public BindGroupLayoutWrapper CreateBindGroupLayout(ref readonly Silk.NET.WebGPU.BindGroupLayoutDescriptor descriptor)
        {
            var result = Api.Core.DeviceCreateBindGroupLayout(Handle, in descriptor);
            if (result == null)
                return null;
            var resultWrapper = new BindGroupLayoutWrapper(Api, result);
            BindGroupLayoutWrapperCreated(resultWrapper);
            return resultWrapper;
        }

        partial void BufferWrapperCreated(BufferWrapper value);
        public BufferWrapper CreateBuffer(ref readonly Silk.NET.WebGPU.BufferDescriptor descriptor)
        {
            var result = Api.Core.DeviceCreateBuffer(Handle, in descriptor);
            if (result == null)
                return null;
            var resultWrapper = new BufferWrapper(Api, result);
            BufferWrapperCreated(resultWrapper);
            return resultWrapper;
        }

        partial void CommandEncoderWrapperCreated(CommandEncoderWrapper value);
        public CommandEncoderWrapper CreateCommandEncoder(ref readonly Silk.NET.WebGPU.CommandEncoderDescriptor descriptor)
        {
            var result = Api.Core.DeviceCreateCommandEncoder(Handle, in descriptor);
            if (result == null)
                return null;
            var resultWrapper = new CommandEncoderWrapper(Api, result);
            CommandEncoderWrapperCreated(resultWrapper);
            return resultWrapper;
        }

        public CommandEncoderWrapper CreateCommandEncoder()
        {
            Silk.NET.WebGPU.CommandEncoderDescriptor descriptor = new Silk.NET.WebGPU.CommandEncoderDescriptor();
            var result = Api.Core.DeviceCreateCommandEncoder(Handle, in descriptor);
            if (result == null)
                return null;
            var resultWrapper = new CommandEncoderWrapper(Api, result);
            CommandEncoderWrapperCreated(resultWrapper);
            return resultWrapper;
        }

        partial void ComputePipelineWrapperCreated(ComputePipelineWrapper value);
        public ComputePipelineWrapper CreateComputePipeline(ref readonly Silk.NET.WebGPU.ComputePipelineDescriptor descriptor)
        {
            var result = Api.Core.DeviceCreateComputePipeline(Handle, in descriptor);
            if (result == null)
                return null;
            var resultWrapper = new ComputePipelineWrapper(Api, result);
            ComputePipelineWrapperCreated(resultWrapper);
            return resultWrapper;
        }

        public void CreateComputePipelineAsync<T0>(ref readonly Silk.NET.WebGPU.ComputePipelineDescriptor descriptor, CreateComputePipelineAsyncCallback callback, ref T0 userdata)
            where T0 : unmanaged
        {
            Silk.NET.WebGPU.PfnCreateComputePipelineAsyncCallback callbackPfn = new Silk.NET.WebGPU.PfnCreateComputePipelineAsyncCallback((arg0, arg1, arg2, arg3) =>
            {
                callback(arg0, arg1 == default ? null : (new ComputePipelineWrapper(Api, arg1)), arg2, arg3);
            });
            Api.Core.DeviceCreateComputePipelineAsync(Handle, in descriptor, callbackPfn, ref userdata);
        }

        partial void PipelineLayoutWrapperCreated(PipelineLayoutWrapper value);
        public PipelineLayoutWrapper CreatePipelineLayout(ref readonly Silk.NET.WebGPU.PipelineLayoutDescriptor descriptor)
        {
            var result = Api.Core.DeviceCreatePipelineLayout(Handle, in descriptor);
            if (result == null)
                return null;
            var resultWrapper = new PipelineLayoutWrapper(Api, result);
            PipelineLayoutWrapperCreated(resultWrapper);
            return resultWrapper;
        }

        partial void QuerySetWrapperCreated(QuerySetWrapper value);
        public QuerySetWrapper CreateQuerySet(ref readonly Silk.NET.WebGPU.QuerySetDescriptor descriptor)
        {
            var result = Api.Core.DeviceCreateQuerySet(Handle, in descriptor);
            if (result == null)
                return null;
            var resultWrapper = new QuerySetWrapper(Api, result);
            QuerySetWrapperCreated(resultWrapper);
            return resultWrapper;
        }

        partial void RenderBundleEncoderWrapperCreated(RenderBundleEncoderWrapper value);
        public RenderBundleEncoderWrapper CreateRenderBundleEncoder(ref readonly Silk.NET.WebGPU.RenderBundleEncoderDescriptor descriptor)
        {
            var result = Api.Core.DeviceCreateRenderBundleEncoder(Handle, in descriptor);
            if (result == null)
                return null;
            var resultWrapper = new RenderBundleEncoderWrapper(Api, result);
            RenderBundleEncoderWrapperCreated(resultWrapper);
            return resultWrapper;
        }

        partial void RenderPipelineWrapperCreated(RenderPipelineWrapper value);
        public RenderPipelineWrapper CreateRenderPipeline(ref readonly Silk.NET.WebGPU.RenderPipelineDescriptor descriptor)
        {
            var result = Api.Core.DeviceCreateRenderPipeline(Handle, in descriptor);
            if (result == null)
                return null;
            var resultWrapper = new RenderPipelineWrapper(Api, result);
            RenderPipelineWrapperCreated(resultWrapper);
            return resultWrapper;
        }

        public void CreateRenderPipelineAsync<T0>(ref readonly Silk.NET.WebGPU.RenderPipelineDescriptor descriptor, CreateRenderPipelineAsyncCallback callback, ref T0 userdata)
            where T0 : unmanaged
        {
            Silk.NET.WebGPU.PfnCreateRenderPipelineAsyncCallback callbackPfn = new Silk.NET.WebGPU.PfnCreateRenderPipelineAsyncCallback((arg0, arg1, arg2, arg3) =>
            {
                callback(arg0, arg1 == default ? null : (new RenderPipelineWrapper(Api, arg1)), arg2, arg3);
            });
            Api.Core.DeviceCreateRenderPipelineAsync(Handle, in descriptor, callbackPfn, ref userdata);
        }

        partial void SamplerWrapperCreated(SamplerWrapper value);
        public SamplerWrapper CreateSampler(ref readonly Silk.NET.WebGPU.SamplerDescriptor descriptor)
        {
            var result = Api.Core.DeviceCreateSampler(Handle, in descriptor);
            if (result == null)
                return null;
            var resultWrapper = new SamplerWrapper(Api, result);
            SamplerWrapperCreated(resultWrapper);
            return resultWrapper;
        }

        partial void ShaderModuleWrapperCreated(ShaderModuleWrapper value);
        public ShaderModuleWrapper CreateShaderModule(ref readonly Silk.NET.WebGPU.ShaderModuleDescriptor descriptor)
        {
            var result = Api.Core.DeviceCreateShaderModule(Handle, in descriptor);
            if (result == null)
                return null;
            var resultWrapper = new ShaderModuleWrapper(Api, result);
            ShaderModuleWrapperCreated(resultWrapper);
            return resultWrapper;
        }

        partial void TextureWrapperCreated(TextureWrapper value);
        public TextureWrapper CreateTexture(ref readonly Silk.NET.WebGPU.TextureDescriptor descriptor)
        {
            var result = Api.Core.DeviceCreateTexture(Handle, in descriptor);
            if (result == null)
                return null;
            var resultWrapper = new TextureWrapper(Api, result);
            TextureWrapperCreated(resultWrapper);
            return resultWrapper;
        }

        public void Destroy()
        {
            Api.Core.DeviceDestroy(Handle);
        }

        public nuint EnumerateFeatures(ref Silk.NET.WebGPU.FeatureName features)
        {
            var result = Api.Core.DeviceEnumerateFeatures(Handle, ref features);
            return result;
        }

        public Silk.NET.Core.Bool32 GetLimits(ref Silk.NET.WebGPU.SupportedLimits limits)
        {
            var result = Api.Core.DeviceGetLimits(Handle, ref limits);
            return result;
        }

        partial void QueueWrapperCreated(QueueWrapper value);
        public QueueWrapper GetQueue()
        {
            var result = Api.Core.DeviceGetQueue(Handle);
            if (result == null)
                return null;
            var resultWrapper = new QueueWrapper(Api, result);
            QueueWrapperCreated(resultWrapper);
            return resultWrapper;
        }

        public Silk.NET.Core.Bool32 HasFeature(Silk.NET.WebGPU.FeatureName feature)
        {
            var result = Api.Core.DeviceHasFeature(Handle, feature);
            return result;
        }

        public void PopErrorScope<T0>(ErrorCallback callback, ref T0 userdata)
            where T0 : unmanaged
        {
            Silk.NET.WebGPU.PfnErrorCallback callbackPfn = new Silk.NET.WebGPU.PfnErrorCallback((arg0, arg1, arg2) =>
            {
                callback(arg0, arg1, arg2);
            });
            Api.Core.DevicePopErrorScope(Handle, callbackPfn, ref userdata);
        }

        public void PushErrorScope(Silk.NET.WebGPU.ErrorFilter filter)
        {
            Api.Core.DevicePushErrorScope(Handle, filter);
        }

        public void SetLabel(ref readonly byte label)
        {
            Api.Core.DeviceSetLabel(Handle, in label);
        }

        public void SetLabel(string label)
        {
            Api.Core.DeviceSetLabel(Handle, label);
        }

        public void SetUncapturedErrorCallback<T0>(ErrorCallback callback, ref T0 userdata)
            where T0 : unmanaged
        {
            Silk.NET.WebGPU.PfnErrorCallback callbackPfn = new Silk.NET.WebGPU.PfnErrorCallback((arg0, arg1, arg2) =>
            {
                callback(arg0, arg1, arg2);
            });
            Api.Core.DeviceSetUncapturedErrorCallback(Handle, callbackPfn, ref userdata);
        }

        public void Reference()
        {
            Api.Core.DeviceReference(Handle);
        }

        public static implicit operator Silk.NET.WebGPU.Device*(DeviceWrapper deviceWrapper) => deviceWrapper.Handle;
        public void Dispose()
        {
            if (Handle == default)
                return;
            Disposing();
            Api.Core.DeviceRelease(Handle);
            Disposed();
        }

        partial void Disposing();
        partial void Disposed();
    }
}