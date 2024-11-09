namespace SilkWrapped.WebGPU
{
    public unsafe partial class QuerySetWrapper : System.IDisposable
    {
        public ApiContainer Api { get; }
        public Silk.NET.WebGPU.QuerySet* Handle { get; }

        public QuerySetWrapper(ApiContainer api, Silk.NET.WebGPU.QuerySet* handle)
        {
            Api = api;
            Handle = handle;
        }

        public void Destroy()
        {
            Api.Core.QuerySetDestroy(Handle);
        }

        public uint GetCount()
        {
            var result = Api.Core.QuerySetGetCount(Handle);
            return result;
        }

        public Silk.NET.WebGPU.QueryType GetType()
        {
            var result = Api.Core.QuerySetGetType(Handle);
            return result;
        }

        public void SetLabel(ref readonly byte label)
        {
            Api.Core.QuerySetSetLabel(Handle, in label);
        }

        public void SetLabel(string label)
        {
            Api.Core.QuerySetSetLabel(Handle, label);
        }

        public void Reference()
        {
            Api.Core.QuerySetReference(Handle);
        }

        public static implicit operator Silk.NET.WebGPU.QuerySet*(QuerySetWrapper querySetWrapper) => querySetWrapper.Handle;
        public void Dispose()
        {
            if (Handle == default)
                return;
            Disposing();
            Api.Core.QuerySetRelease(Handle);
            Disposed();
        }

        partial void Disposing();
        partial void Disposed();
    }
}