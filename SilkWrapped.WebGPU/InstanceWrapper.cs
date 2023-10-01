namespace SilkWrapped.WebGPU;
public unsafe partial class InstanceWrapper
{
    public Task<AdapterWrapper> RequestAdapterAsync(SurfaceWrapper surface, PowerPreference powerPreference = PowerPreference.HighPerformance)
    {
        int dummy = 0;
        var tcs = new TaskCompletionSource<AdapterWrapper>();

        RequestAdapter(new RequestAdapterOptions() { CompatibleSurface = surface, PowerPreference = powerPreference },
        new PfnRequestAdapterCallback((arg0, arg1, arg2, arg3) =>
        {
            if (arg0 == RequestAdapterStatus.Success)
            {
                var adapter = new AdapterWrapper(Api, arg1);
                tcs.SetResult(adapter);
            }
            else
            {
                tcs.SetException(new Exception($"Error requesting adapter. {SilkMarshal.PtrToString((nint)arg2)}"));
            }
        }),
        ref dummy);

        return tcs.Task;
    }
}
