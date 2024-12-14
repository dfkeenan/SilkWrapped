namespace SilkWrapped.WebGPU;
public unsafe partial class AdapterWrapper
{
    public DeviceWrapper RequestDevice(in DeviceDescriptor descriptor = default)
    {
        var resetEvent = new ManualResetEvent(false);

        DeviceWrapper? device = null;
        Exception? exception = null;

        RequestDevice(in descriptor, (arg0, arg1, arg2, arg3) =>
        {
            if (arg0 == RequestDeviceStatus.Success)
            {
                device = arg1;
            }
            else
            {
                exception = new Exception($"Error requesting adapter. {arg2}");
            }

            resetEvent.Set();
        });

        resetEvent.WaitOne();

        if (exception != null)
        {
            throw exception;
        }

        return device;
    }
}
