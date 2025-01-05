namespace SilkWrapped.WebGPU;
public unsafe partial class Adapter
{
    public Device RequestDevice(in DeviceDescriptor descriptor = default)
    {
        var resetEvent = new ManualResetEvent(false);

        Device? device = null;
        Exception? exception = null;
        using PfnRequestDeviceCallback callback = new((status, handle, message, arg3) =>
        {
            if (status == RequestDeviceStatus.Success)
            {
                device = new Device(WebGPU, handle);
            }
            else
            {
                exception = new Exception($"Error requesting adapter. {message}");
            }

            resetEvent.Set();
        });

        RequestDevice(in descriptor, callback);

        resetEvent.WaitOne();

        if (exception != null)
        {
            throw exception;
        }

        return device;
    }
}
