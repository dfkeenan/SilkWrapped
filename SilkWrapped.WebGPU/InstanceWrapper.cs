namespace SilkWrapped.WebGPU;
public unsafe partial class InstanceWrapper
{
    public AdapterWrapper RequestAdapter(SurfaceWrapper surface, PowerPreference powerPreference = PowerPreference.HighPerformance)
    {
        var resetEvent = new ManualResetEvent(false);

        AdapterWrapper? adapter = null;
        Exception? exception = null;

        RequestAdapterOptions options = new RequestAdapterOptions() { CompatibleSurface = surface, PowerPreference = powerPreference };
        PfnRequestAdapterCallback callback = new((arg0, arg1, arg2, arg3) =>
        {
            if (arg0 == RequestAdapterStatus.Success)
            {
                adapter = new AdapterWrapper(this.WebGPU, arg1);
            }
            else
            {
                exception = new Exception($"Error requesting adapter. {arg2}");
            }

            resetEvent.Set();
        });



        this.RequestAdapter(in options, callback);
        
        resetEvent.WaitOne();

        if (exception != null)
        {
            throw exception;
        }

        return adapter;
    }
}