using Silk.NET.WebGPU;

namespace SilkWrapped.WebGPU;
public unsafe partial class InstanceWrapper
{
    public AdapterWrapper RequestAdapter(SurfaceWrapper surface, PowerPreference powerPreference = PowerPreference.HighPerformance)
    {
        int dummy = 0;
        var resetEvent = new ManualResetEvent(false);

        AdapterWrapper? adapter = null;
        Exception? exception = null;

        RequestAdapterOptions options = new RequestAdapterOptions() { CompatibleSurface = surface, PowerPreference = powerPreference };
        this.RequestAdapter<int>(in options,
        (arg0, arg1, arg2, arg3) =>
        {
            if (arg0 == RequestAdapterStatus.Success)
            {
                adapter = arg1;
            }
            else
            {
                exception = new Exception($"Error requesting adapter. {SilkMarshal.PtrToString((nint)arg2)}");
            }

            resetEvent.Set();
        },
        ref dummy);

        resetEvent.WaitOne();

        if (exception != null)
        {
            throw exception;
        }

        return adapter;
    }
}