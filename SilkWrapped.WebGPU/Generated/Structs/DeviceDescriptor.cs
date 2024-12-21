using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct DeviceDescriptor
{
    public unsafe string? Label;
    public unsafe FeatureName[]? RequiredFeatures;
    public unsafe RequiredLimits? RequiredLimits;
    public QueueDescriptor DefaultQueue;
    public PfnDeviceLostCallback DeviceLostCallback;
    public unsafe void* DeviceLostUserdata;
    public unsafe DeviceDescriptor(string? label = null, FeatureName[]? requiredFeatures = null, RequiredLimits? requiredLimits = null, QueueDescriptor? defaultQueue = null, PfnDeviceLostCallback? deviceLostCallback = null, void* deviceLostUserdata = null)
    {
        this = default(DeviceDescriptor);
        if (label != null)
        {
            Label = label;
        }

        if (requiredFeatures != null)
        {
            RequiredFeatures = requiredFeatures;
        }

        if (requiredLimits.HasValue)
        {
            RequiredLimits = requiredLimits.Value;
        }

        if (defaultQueue.HasValue)
        {
            DefaultQueue = defaultQueue.Value;
        }

        if (deviceLostCallback.HasValue)
        {
            DeviceLostCallback = deviceLostCallback.Value;
        }

        if (deviceLostUserdata != null)
        {
            DeviceLostUserdata = deviceLostUserdata;
        }
    }
}