using System.Diagnostics.CodeAnalysis;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct QuerySetHandle : IEquatable<QuerySetHandle>
{
    private readonly nint nativeHandle;
    private QuerySetHandle(Silk.NET.WebGPU.QuerySet* nativeHandle)
    {
        this.nativeHandle = (nint)nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.QuerySet*(QuerySetHandle handle) => (Silk.NET.WebGPU.QuerySet*)handle.nativeHandle;
    public static implicit operator QuerySetHandle(Silk.NET.WebGPU.QuerySet* handle) => new QuerySetHandle(handle);
    public static bool operator ==(QuerySetHandle handle, QuerySetHandle other) => handle.nativeHandle == other.nativeHandle;
    public static bool operator !=(QuerySetHandle handle, QuerySetHandle other) => handle.nativeHandle != other.nativeHandle;
    public bool Equals(QuerySetHandle other) => this == other;
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not QuerySetHandle other)
            return false;
        return this == other;
    }

    public override int GetHashCode() => (int)nativeHandle;
}

public unsafe partial class QuerySet : IEquatable<QuerySet>, System.IDisposable
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

    public bool Equals([NotNullWhen(true)] QuerySet? other)
    {
        if (other is null)
            return false;
        return Handle == other.Handle;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not QuerySet other)
            return false;
        return Handle == other.Handle;
    }

    public override int GetHashCode() => Handle.GetHashCode();
    public void  Dispose()
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