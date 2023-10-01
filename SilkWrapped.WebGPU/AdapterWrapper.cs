namespace SilkWrapped.WebGPU;
public unsafe partial class AdapterWrapper
{
    public Task<DeviceWrapper> RequestDeviceAsync(in DeviceDescriptor descriptor = default)
    {
        int dummy = 0;
        var tcs = new TaskCompletionSource<DeviceWrapper>();

        RequestDevice(descriptor,
        new PfnRequestDeviceCallback((arg0, arg1, arg2, arg3) =>
        {
            if (arg0 == RequestDeviceStatus.Success)
            {
                var device = new DeviceWrapper(Api, arg1);
                tcs.SetResult(device);
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
