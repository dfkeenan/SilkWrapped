namespace SilkWrapped.WebGPU;
public unsafe readonly struct RenderPipelineHandle
{
    private readonly Silk.NET.WebGPU.RenderPipeline* nativeHandle;
    private RenderPipelineHandle(Silk.NET.WebGPU.RenderPipeline* nativeHandle)
    {
        this.nativeHandle = nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.RenderPipeline*(RenderPipelineHandle handle) => handle.nativeHandle;
    public static implicit operator RenderPipelineHandle(Silk.NET.WebGPU.RenderPipeline* handle) => new RenderPipelineHandle(handle);
}

public unsafe partial class RenderPipeline : System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public RenderPipelineHandle Handle { get; private set; }

    public RenderPipeline(Silk.NET.WebGPU.WebGPU webGPU, RenderPipelineHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator RenderPipelineHandle(RenderPipeline obj) => obj.Handle;
    public unsafe BindGroupLayout GetBindGroupLayout(uint groupIndex)
    {
        var result = WebGPU.RenderPipelineGetBindGroupLayout(Handle, groupIndex);
        return new BindGroupLayout(WebGPU, result);
    }

    public unsafe void SetLabel(string label)
    {
        using var m = new MarshalHelper();
        WebGPU.RenderPipelineSetLabel(Handle, m.RentUtf8Ptr(label));
    }

    public unsafe void Reference()
    {
        WebGPU.RenderPipelineReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.RenderPipelineRelease(Handle);
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