namespace SilkWrapped.WebGPU
{
    public unsafe partial class AdapterWrapper : System.IDisposable
    {
        public ApiContainer Api { get; }
        public Silk.NET.WebGPU.Adapter* Handle { get; }

        public AdapterWrapper(ApiContainer api, Silk.NET.WebGPU.Adapter* handle)
        {
            Api = api;
            Handle = handle;
        }

        public nuint EnumerateFeatures(ref Silk.NET.WebGPU.FeatureName features)
        {
            var result = Api.Core.AdapterEnumerateFeatures(Handle, ref features);
            return result;
        }

        public Silk.NET.Core.Bool32 GetLimits(ref Silk.NET.WebGPU.SupportedLimits limits)
        {
            var result = Api.Core.AdapterGetLimits(Handle, ref limits);
            return result;
        }

        public void GetProperties(ref Silk.NET.WebGPU.AdapterProperties properties)
        {
            Api.Core.AdapterGetProperties(Handle, ref properties);
        }

        public Silk.NET.Core.Bool32 HasFeature(Silk.NET.WebGPU.FeatureName feature)
        {
            var result = Api.Core.AdapterHasFeature(Handle, feature);
            return result;
        }

        public void RequestInfo<T0>(AdapterRequestAdapterInfoCallback callback, ref T0 userdata)
            where T0 : unmanaged
        {
            Silk.NET.WebGPU.PfnAdapterRequestAdapterInfoCallback callbackPfn = new Silk.NET.WebGPU.PfnAdapterRequestAdapterInfoCallback((arg0, arg1) =>
            {
                callback(arg0, arg1);
            });
            Api.Core.AdapterRequestAdapterInfo(Handle, callbackPfn, ref userdata);
        }

        public void RequestDevice<T0>(ref readonly Silk.NET.WebGPU.DeviceDescriptor descriptor, RequestDeviceCallback callback, ref T0 userdata)
            where T0 : unmanaged
        {
            Silk.NET.WebGPU.PfnRequestDeviceCallback callbackPfn = new Silk.NET.WebGPU.PfnRequestDeviceCallback((arg0, arg1, arg2, arg3) =>
            {
                callback(arg0, arg1 == default ? null : (new DeviceWrapper(Api, arg1)), arg2, arg3);
            });
            Api.Core.AdapterRequestDevice(Handle, in descriptor, callbackPfn, ref userdata);
        }

        public void Reference()
        {
            Api.Core.AdapterReference(Handle);
        }

        public static implicit operator Silk.NET.WebGPU.Adapter*(AdapterWrapper adapterWrapper) => adapterWrapper.Handle;
        public void Dispose()
        {
            if (Handle == default)
                return;
            Disposing();
            Api.Core.AdapterRelease(Handle);
            Disposed();
        }

        partial void Disposing();
        partial void Disposed();
    }
}