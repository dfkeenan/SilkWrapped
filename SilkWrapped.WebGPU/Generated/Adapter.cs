using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using Silk.NET.Core;
using Silk.NET.Core.Attributes;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;

namespace SilkWrapped.WebGPU;
public unsafe readonly struct AdapterHandle
{
    private readonly Silk.NET.WebGPU.Adapter* nativeHandle;
    private AdapterHandle(Silk.NET.WebGPU.Adapter* nativeHandle)
    {
        this.nativeHandle = nativeHandle;
    }

    public bool IsEmpty => nativeHandle == default;

    public static implicit operator Silk.NET.WebGPU.Adapter*(AdapterHandle handle) => handle.nativeHandle;
    public static implicit operator AdapterHandle(Silk.NET.WebGPU.Adapter* handle) => new AdapterHandle(handle);
}

public unsafe partial class Adapter : System.IDisposable
{
    public Silk.NET.WebGPU.WebGPU WebGPU { get; }
    public AdapterHandle Handle { get; private set; }

    public Adapter(Silk.NET.WebGPU.WebGPU webGPU, AdapterHandle handle)
    {
        WebGPU = webGPU;
        Handle = handle;
    }

    public static implicit operator AdapterHandle(Adapter obj) => obj.Handle;
    public unsafe nuint EnumerateFeatures(ref FeatureName features)
    {
        var result = WebGPU.AdapterEnumerateFeatures(Handle, ref Unsafe.As<FeatureName, Silk.NET.WebGPU.FeatureName>(ref features));
        return result;
    }

    public unsafe Bool32 GetLimits(ref SupportedLimits limits)
    {
        Silk.NET.WebGPU.SupportedLimits __limits = default;
        var result = WebGPU.AdapterGetLimits(Handle, ref __limits);
        limits.Limits = Unsafe.BitCast<Silk.NET.WebGPU.Limits, Limits>(__limits.Limits);
        return result;
    }

    public unsafe void GetProperties(ref AdapterProperties properties)
    {
        Silk.NET.WebGPU.AdapterProperties __properties = default;
        WebGPU.AdapterGetProperties(Handle, ref __properties);
        properties.VendorID = __properties.VendorID;
        properties.VendorName = SilkMarshal.PtrToString((nint)__properties.VendorName, NativeStringEncoding.UTF8);
        properties.Architecture = SilkMarshal.PtrToString((nint)__properties.Architecture, NativeStringEncoding.UTF8);
        properties.DeviceID = __properties.DeviceID;
        properties.Name = SilkMarshal.PtrToString((nint)__properties.Name, NativeStringEncoding.UTF8);
        properties.DriverDescription = SilkMarshal.PtrToString((nint)__properties.DriverDescription, NativeStringEncoding.UTF8);
        properties.AdapterType = (AdapterType)__properties.AdapterType;
        properties.BackendType = (BackendType)__properties.BackendType;
    }

    public unsafe Bool32 HasFeature(FeatureName feature)
    {
        var result = WebGPU.AdapterHasFeature(Handle, (Silk.NET.WebGPU.FeatureName)feature);
        return result;
    }

    public unsafe void RequestInfo(PfnAdapterRequestAdapterInfoCallback callback)
    {
        WebGPU.AdapterRequestAdapterInfo(Handle, callback, null);
    }

    public unsafe void RequestInfo<T0>(PfnAdapterRequestAdapterInfoCallback callback, ref T0 userdata)
        where T0 : unmanaged
    {
        WebGPU.AdapterRequestAdapterInfo<T0>(Handle, callback, ref userdata);
    }

    public unsafe void RequestDevice(in DeviceDescriptor descriptor, PfnRequestDeviceCallback callback)
    {
        using var m = new MarshalHelper();
        Silk.NET.WebGPU.DeviceDescriptor __descriptor = default;
        __descriptor.Label = m.RentUtf8Ptr(descriptor.Label);
        if (descriptor.RequiredFeatures != null)
        {
            __descriptor.RequiredFeatureCount = (nuint)descriptor.RequiredFeatures!.Length;
            __descriptor.RequiredFeatures = m.Pin<FeatureName, Silk.NET.WebGPU.FeatureName>(descriptor.RequiredFeatures);
        }

        Silk.NET.WebGPU.RequiredLimits __descriptor_RequiredLimits = default;
        if (descriptor.RequiredLimits.HasValue)
        {
            __descriptor_RequiredLimits.Limits = Unsafe.BitCast<Limits, Silk.NET.WebGPU.Limits>(descriptor.RequiredLimits!.Value.Limits);
            __descriptor.RequiredLimits = &__descriptor_RequiredLimits; //PINREF
        }

        __descriptor.DefaultQueue.Label = m.RentUtf8Ptr(descriptor.DefaultQueue.Label);
        __descriptor.DeviceLostCallback = descriptor.DeviceLostCallback;
        WebGPU.AdapterRequestDevice(Handle, in __descriptor, callback, null);
    }

    public unsafe void RequestDevice<T0>(in DeviceDescriptor descriptor, PfnRequestDeviceCallback callback, ref T0 userdata)
        where T0 : unmanaged
    {
        using var m = new MarshalHelper();
        Silk.NET.WebGPU.DeviceDescriptor __descriptor = default;
        __descriptor.Label = m.RentUtf8Ptr(descriptor.Label);
        if (descriptor.RequiredFeatures != null)
        {
            __descriptor.RequiredFeatureCount = (nuint)descriptor.RequiredFeatures!.Length;
            __descriptor.RequiredFeatures = m.Pin<FeatureName, Silk.NET.WebGPU.FeatureName>(descriptor.RequiredFeatures);
        }

        Silk.NET.WebGPU.RequiredLimits __descriptor_RequiredLimits = default;
        if (descriptor.RequiredLimits.HasValue)
        {
            __descriptor_RequiredLimits.Limits = Unsafe.BitCast<Limits, Silk.NET.WebGPU.Limits>(descriptor.RequiredLimits!.Value.Limits);
            __descriptor.RequiredLimits = &__descriptor_RequiredLimits; //PINREF
        }

        __descriptor.DefaultQueue.Label = m.RentUtf8Ptr(descriptor.DefaultQueue.Label);
        __descriptor.DeviceLostCallback = descriptor.DeviceLostCallback;
        WebGPU.AdapterRequestDevice<T0>(Handle, in __descriptor, callback, ref userdata);
    }

    public unsafe void Reference()
    {
        WebGPU.AdapterReference(Handle);
    }

    public unsafe void Release()
    {
        WebGPU.AdapterRelease(Handle);
    }

    public void  Dispose()
    {
        if (Handle.IsEmpty)
            return;
        Disposing();
        Release();
        Handle = default;
        Disposed();
    }

    partial void Disposing();
    partial void Disposed();
}