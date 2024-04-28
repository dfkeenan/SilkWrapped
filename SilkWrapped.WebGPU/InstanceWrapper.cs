namespace SilkWrapped.WebGPU;
public unsafe partial class InstanceWrapper
{
    public Task<AdapterWrapper> RequestAdapterAsync(SurfaceWrapper surface, PowerPreference powerPreference = PowerPreference.HighPerformance)
    {
        int dummy = 0;
        var tcs = new TaskCompletionSource<AdapterWrapper>();

        RequestAdapterOptions options = new RequestAdapterOptions() { CompatibleSurface = surface, PowerPreference = powerPreference };
        this.RequestAdapter<int>(in options,
        (arg0, arg1, arg2, arg3) =>
        {
            if (arg0 == RequestAdapterStatus.Success)
            { 
                tcs.SetResult(arg1);
            }
            else
            {
                tcs.SetException(new Exception($"Error requesting adapter. {SilkMarshal.PtrToString((nint)arg2)}"));
            }
        },
        ref dummy);

        return tcs.Task;
    }
}
