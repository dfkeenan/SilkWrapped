namespace SilkWrapped.WebGPU;
public unsafe readonly struct ShaderModuleHandle
{
    private readonly Silk.NET.WebGPU.ShaderModule* nativeHandle;
    private ShaderModuleHandle(Silk.NET.WebGPU.ShaderModule* nativeHandle)
    {
        this.nativeHandle = nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.ShaderModule*(ShaderModuleHandle handle) => handle.nativeHandle;
    public static implicit operator ShaderModuleHandle(Silk.NET.WebGPU.ShaderModule* handle) => new ShaderModuleHandle(handle);
}

public unsafe partial class ShaderModule : System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public ShaderModuleHandle Handle { get; private set; }

    public ShaderModule(Silk.NET.WebGPU.WebGPU webGPU, ShaderModuleHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator ShaderModuleHandle(ShaderModule obj) => obj.Handle;
    public unsafe void GetCompilationInfo(PfnCompilationInfoCallback callback)
    {
        WebGPU.ShaderModuleGetCompilationInfo(Handle, callback, null);
    }

    public unsafe void GetCompilationInfo<T0>(PfnCompilationInfoCallback callback, ref T0 userdata)
        where T0 : unmanaged
    {
        WebGPU.ShaderModuleGetCompilationInfo<T0>(Handle, callback, ref userdata);
    }

    public unsafe void SetLabel(string label)
    {
        using var m = new MarshalHelper();
        WebGPU.ShaderModuleSetLabel(Handle, m.RentUtf8Ptr(label));
    }

    public unsafe void Reference()
    {
        WebGPU.ShaderModuleReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.ShaderModuleRelease(Handle);
    }

    public void Dispose()
    {
        if (Handle.IsEmpty)
            return;
        Disposing();
        Release();
        Handle = default;
        Disposed();
    }

    partial void Disposing();
    partial void Disposed();
}