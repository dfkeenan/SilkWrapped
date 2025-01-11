using System.Runtime.CompilerServices;
using Silk.NET.Windowing;

namespace SilkWrapped.WebGPU;

public struct DeviceManagerOptions
{
    public PowerPreference PowerPreference = PowerPreference.HighPerformance;
    public BackendType BackendType = BackendType.Undefined;
    public PresentMode PresentMode = PresentMode.Fifo;
    public TextureFormat? SurfaceFormat = null;
    public TextureFormat? DepthStencilFormat = null;

    public static DeviceManagerOptions Default => new();

    public DeviceManagerOptions()
    {
    }
}

public readonly record struct FramebufferSize(uint Width, uint Height)
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
    private readonly Instance instance;
    private Adapter adapter;
    private PfnDeviceLostCallback deviceLostCallback;
    private PfnErrorCallback errorCallback;
    private PresentMode presentMode;

    private TextureView? currentSurfaceTextureView;
    private RenderPassEncoder? currentRenderPassEncoder;
    private CommandEncoder? currentCommandEncoder;
    private Texture? depthStencilTexture;
    private TextureView? depthStencilTextureView;

    private bool isDisposed;

    public SurfaceCapabilities SurfaceCapabilities { get; private set; }
    public Queue Queue
    {
        get => field ?? throw new InvalidOperationException($"{nameof(GraphicsDeviceManager)} has not been loaded.");
        private set;
    }

    public Device Device
    {
        get => field ?? throw new InvalidOperationException($"{nameof(GraphicsDeviceManager)} has not been loaded.");
        private set;
    }

    public Surface Surface
    {
        get => field ?? throw new InvalidOperationException($"{nameof(GraphicsDeviceManager)} has not been loaded.");
        private set;
    }

    public TextureFormat SurfaceTextureFormat { get; private set; }
    public TextureFormat? DepthStencilTextureFormat { get; private set; }

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

        instance = new Instance();

        if (!view.IsInitialized)
        {
            view.Load += Load;
        }
    }

    public void Load()
    {
        view.Load -= Load;
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

        DeviceDescriptor deviceDescriptor = new()
        {
            DeviceLostCallback = deviceLostCallback,
        };

        Device = adapter.RequestDevice(in deviceDescriptor);

        Device.SetUncapturedErrorCallback(errorCallback);

        Queue = Device.GetQueue();

        if (options.SurfaceFormat is TextureFormat format)
        {
            if (surfaceCapabilities.Formats?.Contains(format) ?? false)
            {
                throw new InvalidOperationException($"Unsuppoted surface format {format}");
            }

            SurfaceTextureFormat = format;
        }
        else
        {
            SurfaceTextureFormat = SurfaceCapabilities.Formats![0];
        }

        if (options.DepthStencilFormat is TextureFormat depthFormat)
        {
            //TODO: DepthStencilFormat validation
            if (depthFormat is
                TextureFormat.Depth24Plus or
                TextureFormat.Depth24PlusStencil8 or
                TextureFormat.Depth32float)
            {
                DepthStencilTextureFormat = depthFormat;
            }
            //var features = Device.EnumerateFeatures();
        }

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
    {
        CreateSwapChain();

        depthStencilTextureView?.Dispose();
        depthStencilTextureView = null;
        depthStencilTexture?.Dispose();
        depthStencilTexture = null;
    }

    public void CreateSwapChain()
    {
        var surfaceConfiguration = new SurfaceConfiguration
        {
            Usage = TextureUsage.RenderAttachment,
            Format = SurfaceTextureFormat,
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

    private TextureView? GetDepthTexture()
    {
        if (DepthStencilTextureFormat is not TextureFormat depthFormat) return null;

        if (depthStencilTextureView is TextureView) return depthStencilTextureView;

        depthStencilTextureView?.Dispose();
        depthStencilTexture?.Dispose();

        depthStencilTexture = Device.CreateTexture(
            view.FramebufferSize,
            depthFormat,
            TextureUsage.RenderAttachment);

        depthStencilTextureView = depthStencilTexture.CreateView();

        return depthStencilTextureView;
    }


    public RenderPassEncoder? TryBeginDraw(
       Color? clearColor = null,
       float? depthClearValue = null)
    {
        if (currentSurfaceTextureView is not null)
        {
            throw new InvalidOperationException($"'{nameof(TryBeginDraw)}' has already been called");
        }

        currentSurfaceTextureView = GetCurrentSurfaceTextureView();
        if (currentSurfaceTextureView is null) return null;

        currentCommandEncoder = Device.CreateCommandEncoder();

        if (GetDepthTexture() is TextureView depthTextureView)
        {
            currentRenderPassEncoder = currentCommandEncoder.BeginRenderPass(
                currentSurfaceTextureView,
                depthTextureView,
                clearColor,
                depthClearValue);
        }
        else
        {
            currentRenderPassEncoder = currentCommandEncoder.BeginRenderPass(currentSurfaceTextureView, clearColor);
        }

        return currentRenderPassEncoder;
    }

    public void EndDraw()
    {
        if (currentCommandEncoder is null)
        {
            throw new InvalidOperationException($"Must call '{TryBeginDraw}' first.");
        }

        currentRenderPassEncoder!.End();
        using var commandBuffer = currentCommandEncoder.Finish();
        Present(commandBuffer);

        currentCommandEncoder?.Dispose();
        currentCommandEncoder = null;
        currentRenderPassEncoder?.Dispose();
        currentRenderPassEncoder = null;
        currentSurfaceTextureView?.Dispose();
        currentSurfaceTextureView = null;
    }

    public void Present(params ReadOnlySpan<CommandBufferHandle> buffers)
    {
        Queue.Submit(buffers);
        Surface.Present();
        view!.SwapBuffers();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            currentCommandEncoder?.Dispose();
            currentCommandEncoder = null;
            currentRenderPassEncoder?.Dispose();
            currentRenderPassEncoder = null;

            depthStencilTextureView?.Dispose();
            depthStencilTexture?.Dispose();

            Queue?.Dispose();
            Device?.Dispose();
            deviceLostCallback.Dispose();
            errorCallback.Dispose();
            adapter?.Dispose();
            Surface?.Dispose();
            instance?.Dispose();

            isDisposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~GraphicsDeviceManager()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
