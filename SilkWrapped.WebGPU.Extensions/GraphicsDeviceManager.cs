using System.Runtime.CompilerServices;
using Silk.NET.Windowing;

namespace SilkWrapped.WebGPU;

public struct DeviceManagerOptions
{
    public PowerPreference PowerPreference = PowerPreference.HighPerformance;
    public BackendType BackendType = BackendType.Undefined;
    public PresentMode PresentMode = PresentMode.Fifo;
    public TextureFormat? SurfaceFormat = null;

    public static DeviceManagerOptions Default => new DeviceManagerOptions();

    public DeviceManagerOptions()
    {
    }
}

public readonly record struct FramebufferSize(uint width, uint height)
{
    public FramebufferSize(int width, int height)
        : this((uint)width, (uint)height)
    {

    }
}

public unsafe partial class GraphicsDeviceManager : IDisposable
{
    private readonly IView view;
    private readonly DeviceManagerOptions options;
    private Instance instance;
    private Adapter adapter;
    private PfnDeviceLostCallback deviceLostCallback;
    private PfnErrorCallback errorCallback;
    private PresentMode presentMode;

    public SurfaceCapabilities SurfaceCapabilities { get; private set; }
    public Queue Queue { get; private set; }
    public Device Device { get; private set; }
    public Surface Surface { get; private set; }
    public TextureFormat DefaultSurfaceFormat { get; private set; }

    public event Action<ErrorType, string?>? UncapturedError;
    public event Action<DeviceLostReason, string?>? DeviceLost;

    public GraphicsDeviceManager(IView view)
        : this(view, DeviceManagerOptions.Default)
    {

    }

    public GraphicsDeviceManager(IView view, DeviceManagerOptions options)
    {
        this.view = view ?? throw new ArgumentNullException(nameof(view));
        this.options = options;
        presentMode = options.PresentMode;
        deviceLostCallback = PfnDeviceLostCallback.From(OnDeviceLost);
        errorCallback = PfnErrorCallback.From(OnError);

        if (!view.IsInitialized)
        {
            view.Load += Load;
        }
    }

    public void Load()
    {
        view.Load -= Load;

        instance = new Instance();
        Surface = view!.CreateWebGPUSurface(instance);

        RequestAdapterOptions adapterOptions = new()
        {
            CompatibleSurface = Surface,
            PowerPreference = options.PowerPreference,
            BackendType = options.BackendType,
        };

        adapter = instance.RequestAdapter(Surface);

        SurfaceCapabilities surfaceCapabilities = default;
        Surface.GetCapabilities(adapter, ref surfaceCapabilities);
        SurfaceCapabilities = surfaceCapabilities;

        if (options.SurfaceFormat is TextureFormat format)
        {
            if (surfaceCapabilities.Formats?.Contains(format) ?? false)
            {
                throw new InvalidOperationException($"Unsuppoted surface format {format}");
            }

            DefaultSurfaceFormat = format;
        }
        else
        {
            DefaultSurfaceFormat = SurfaceCapabilities.Formats![0];
        }


        DeviceDescriptor deviceDescriptor = new()
        {
            DeviceLostCallback = deviceLostCallback,
        };

        Device = adapter.RequestDevice(in deviceDescriptor);

        Device.SetUncapturedErrorCallback(errorCallback);

        Queue = Device.GetQueue();

        CreateSwapChain();
    }

    private unsafe void OnDeviceLost(DeviceLostReason reason, string? message, void* userdata)
    {
        DeviceLost?.Invoke(reason, message);
    }

    private unsafe void OnError(ErrorType reason, string? message, void* userdata)
    {
        UncapturedError?.Invoke(reason, message);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResizeSwapChain()
        => CreateSwapChain();

    public void CreateSwapChain()
    {
        var surfaceConfiguration = new SurfaceConfiguration
        {
            Usage = TextureUsage.RenderAttachment,
            Format = DefaultSurfaceFormat,
            PresentMode = presentMode,
            Device = Device,
            Width = (uint)view.FramebufferSize.X,
            Height = (uint)view.FramebufferSize.Y,
        };

        Surface!.Configure(in surfaceConfiguration);
    }

    public TextureView? GetCurrentSurfaceTextureView()
    {
        var (status, surfaceTexture) = Surface!.GetCurrentTexture();
        switch (status)
        {
            case SurfaceGetCurrentTextureStatus.Success:
                break;
            case SurfaceGetCurrentTextureStatus.Timeout:
            case SurfaceGetCurrentTextureStatus.Outdated:
            case SurfaceGetCurrentTextureStatus.Lost:
                // Recreate swapchain,
                surfaceTexture.Dispose();
                CreateSwapChain();
                // Skip this frame
                return null;
            case SurfaceGetCurrentTextureStatus.OutOfMemory:
            case SurfaceGetCurrentTextureStatus.DeviceLost:
            case SurfaceGetCurrentTextureStatus.Force32:
                throw new Exception($"What is going on bros... {status}");
        }

        return surfaceTexture.CreateView();
    }


    public void Dispose()
    {
        Queue?.Dispose();
        Device?.Dispose();
        deviceLostCallback.Dispose();
        errorCallback.Dispose();
        adapter?.Dispose();
        Surface?.Dispose();
        instance?.Dispose();
    }
}
