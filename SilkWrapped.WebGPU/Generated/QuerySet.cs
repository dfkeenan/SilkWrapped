namespace SilkWrapped.WebGPU;
public unsafe readonly struct QuerySetHandle
{
    private readonly Silk.NET.WebGPU.QuerySet* nativeHandle;
    private QuerySetHandle(Silk.NET.WebGPU.QuerySet* nativeHandle)
    {
        this.nativeHandle = nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.QuerySet*(QuerySetHandle handle) => handle.nativeHandle;
    public static implicit operator QuerySetHandle(Silk.NET.WebGPU.QuerySet* handle) => new QuerySetHandle(handle);
}

public unsafe partial class QuerySet : System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public QuerySetHandle Handle { get; private set; }

    public QuerySet(Silk.NET.WebGPU.WebGPU webGPU, QuerySetHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator QuerySetHandle(QuerySet obj) => obj.Handle;
    public unsafe void Destroy()
    {
        WebGPU.QuerySetDestroy(Handle);
    }

    public unsafe uint GetCount()
    {
        var result = WebGPU.QuerySetGetCount(Handle);
        return result;
    }

    public unsafe QueryType GetType()
    {
        var result = WebGPU.QuerySetGetType(Handle);
        return (QueryType)result;
    }

    public unsafe void SetLabel(string label)
    {
        using var m = new MarshalHelper();
        WebGPU.QuerySetSetLabel(Handle, m.RentUtf8Ptr(label));
    }

    public unsafe void Reference()
    {
        WebGPU.QuerySetReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.QuerySetRelease(Handle);
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