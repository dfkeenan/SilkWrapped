namespace SilkWrapped.WebGPU
{
    public unsafe partial class InstanceWrapper : System.IDisposable
    {
        public ApiContainer Api { get; }
        public Silk.NET.WebGPU.Instance* Handle { get; }

        public InstanceWrapper(ApiContainer api, Silk.NET.WebGPU.Instance* handle)
        {
            Api = api;
            Handle = handle;
        }

        public InstanceWrapper(ref readonly Silk.NET.WebGPU.InstanceDescriptor descriptor)
        {
            Api = new ApiContainer();
            Handle = Api.Core.CreateInstance(in descriptor);
        }

        public InstanceWrapper()
        {
            Silk.NET.WebGPU.InstanceDescriptor descriptor = new Silk.NET.WebGPU.InstanceDescriptor();
            Api = new ApiContainer();
            Handle = Api.Core.CreateInstance(in descriptor);
        }

        partial void SurfaceWrapperCreated(SurfaceWrapper value);
        public SurfaceWrapper CreateSurface(ref readonly Silk.NET.WebGPU.SurfaceDescriptor descriptor)
        {
            var result = Api.Core.InstanceCreateSurface(Handle, in descriptor);
            if (result == null)
                return null;
            var resultWrapper = new SurfaceWrapper(Api, result);
            SurfaceWrapperCreated(resultWrapper);
            return resultWrapper;
        }

        public SurfaceWrapper CreateSurface()
        {
            Silk.NET.WebGPU.SurfaceDescriptor descriptor = new Silk.NET.WebGPU.SurfaceDescriptor();
            var result = Api.Core.InstanceCreateSurface(Handle, in descriptor);
            if (result == null)
                return null;
            var resultWrapper = new SurfaceWrapper(Api, result);
            SurfaceWrapperCreated(resultWrapper);
            return resultWrapper;
        }

        public Silk.NET.Core.Bool32 HasWGSLLanguageFeature(Silk.NET.WebGPU.WGSLFeatureName feature)
        {
            var result = Api.Core.InstanceHasWGSLLanguageFeature(Handle, feature);
            return result;
        }

        public void ProcessEvents()
        {
            Api.Core.InstanceProcessEvents(Handle);
        }

        public void RequestAdapter<T0>(ref readonly Silk.NET.WebGPU.RequestAdapterOptions options, RequestAdapterCallback callback, ref T0 userdata)
            where T0 : unmanaged
        {
            Silk.NET.WebGPU.PfnRequestAdapterCallback callbackPfn = new Silk.NET.WebGPU.PfnRequestAdapterCallback((arg0, arg1, arg2, arg3) =>
            {
                callback(arg0, arg1 == default ? null : (new AdapterWrapper(Api, arg1)), arg2, arg3);
            });
            Api.Core.InstanceRequestAdapter(Handle, in options, callbackPfn, ref userdata);
        }

        public void Reference()
        {
            Api.Core.InstanceReference(Handle);
        }

        public static implicit operator Silk.NET.WebGPU.Instance*(InstanceWrapper instanceWrapper) => instanceWrapper.Handle;
        public void Dispose()
        {
            if (Handle == default)
                return;
            Disposing();
            Api.Core.InstanceRelease(Handle);
            Api.Dispose();
            Disposed();
        }

        partial void Disposing();
        partial void Disposed();
    }
}