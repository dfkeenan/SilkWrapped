namespace SilkWrapped.WebGPU;
public unsafe partial class Instance
{
    public Instance()
    {
        WebGPU = Silk.NET.WebGPU.WebGPU.GetApi();
        Handle = WebGPU.CreateInstance(null);
    }

    public Adapter RequestAdapter(Surface surface, PowerPreference powerPreference = PowerPreference.HighPerformance)
    {
        var resetEvent = new ManualResetEvent(false);

        Adapter? adapter = null;
        Exception? exception = null;

        RequestAdapterOptions options = new RequestAdapterOptions() { CompatibleSurface = surface, PowerPreference = powerPreference };
        using PfnRequestAdapterCallback callback = new((status, handle, message, arg3) =>
        {
            if (status == RequestAdapterStatus.Success)
            {
                adapter = new Adapter(this.WebGPU, handle);
            }
            else
            {
                exception = new Exception($"Error requesting adapter. {message}");
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