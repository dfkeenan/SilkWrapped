namespace SilkWrapped.WebGPU
{
    public unsafe partial class ShaderModuleWrapper : System.IDisposable
    {
        public ApiContainer Api { get; }
        public Silk.NET.WebGPU.ShaderModule* Handle { get; }

        public ShaderModuleWrapper(ApiContainer api, Silk.NET.WebGPU.ShaderModule* handle)
        {
            Api = api;
            Handle = handle;
        }

        public void GetCompilationInfo<T0>(CompilationInfoCallback callback, ref T0 userdata)
            where T0 : unmanaged
        {
            Silk.NET.WebGPU.PfnCompilationInfoCallback callbackPfn = new Silk.NET.WebGPU.PfnCompilationInfoCallback((arg0, arg1, arg2) =>
            {
                callback(arg0, arg1, arg2);
            });
            Api.Core.ShaderModuleGetCompilationInfo(Handle, callbackPfn, ref userdata);
        }

        public void SetLabel(ref readonly byte label)
        {
            Api.Core.ShaderModuleSetLabel(Handle, in label);
        }

        public void SetLabel(string label)
        {
            Api.Core.ShaderModuleSetLabel(Handle, label);
        }

        public void Reference()
        {
            Api.Core.ShaderModuleReference(Handle);
        }

        public static implicit operator Silk.NET.WebGPU.ShaderModule*(ShaderModuleWrapper shaderModuleWrapper) => shaderModuleWrapper.Handle;
        public void Dispose()
        {
            if (Handle == default)
                return;
            Disposing();
            Api.Core.ShaderModuleRelease(Handle);
            Disposed();
        }

        partial void Disposing();
        partial void Disposed();
    }
}