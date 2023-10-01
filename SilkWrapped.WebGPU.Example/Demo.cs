using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Extensions.Dawn;
using Silk.NET.WebGPU.Extensions.WGPU;
using Silk.NET.Windowing;

namespace SilkWrapped.WebGPU.Example;
internal class Demo : IDisposable
{
    private IWindow window = default!;
    private IInputContext input = default!;
    private IKeyboard keyboard;
    private InstanceWrapper instance;
    private SurfaceWrapper surface;
    private SurfaceCapabilities surfaceCapabilities;
    private AdapterWrapper adapter;
    private DeviceWrapper device;
    
    private QueueWrapper queue;
    private SurfaceConfiguration surfaceConfiguration;

    public Demo()
    {
            
    }
    public void Dispose()
    {
        queue?.Dispose();
        
        device?.Dispose();
        adapter?.Dispose();
        surface?.Dispose();
        instance?.Dispose();
        input?.Dispose();
        window?.Dispose();
    }

    internal void Run()
    {
        var options = WindowOptions.Default with 
        { 
            API = GraphicsAPI.None,
            ShouldSwapAutomatically = false,
            IsContextControlDisabled = true,
            Size = new Vector2D<int>(800, 600),
            Title = "WebGPU with Silk.NET",
        };

        window = Window.Create(options);

        //Assign events.
        window.Load += OnLoad;
        window.Update += OnUpdate;
        window.Render += OnRender;

        //Run the window.
        window.Run();
    }

    private async void OnLoad()
    {
        input = window.CreateInput();
        keyboard = input.Keyboards[0];

        instance = new InstanceWrapper();
        surface = window.CreateWebGPUSurface(instance);

        adapter = await instance.RequestAdapterAsync(surface);
        device = await adapter.RequestDeviceAsync();

        surface.GetCapabilities(adapter, ref surfaceCapabilities);

        queue = device.GetQueue();
        CreateSwapChain();
    }

    private unsafe void CreateSwapChain()
    {
        surfaceConfiguration = new SurfaceConfiguration
        {
            Usage = TextureUsage.RenderAttachment,
            Format = surfaceCapabilities.Formats[0],
            PresentMode = PresentMode.Fifo,
            Device = device,
            Width = (uint)window.FramebufferSize.X,
            Height = (uint)window.FramebufferSize.Y
        };

        surface.Configure(surfaceConfiguration);
    }

    private void OnUpdate(double obj)
    {
        if(keyboard.IsKeyPressed(Key.Escape))
        {
            window.Close();
        }
    }

    private unsafe void OnRender(double obj)
    {
        var (status, surfaceTexture) = surface.GetCurrentTexture();
        switch (status)
        {
            case SurfaceGetCurrentTextureStatus.Timeout:
            case SurfaceGetCurrentTextureStatus.Outdated:
            case SurfaceGetCurrentTextureStatus.Lost:
                // Recreate swapchain,
                surfaceTexture.Dispose();
                CreateSwapChain();
                // Skip this frame
                return;
            case SurfaceGetCurrentTextureStatus.OutOfMemory:
            case SurfaceGetCurrentTextureStatus.DeviceLost:
            case SurfaceGetCurrentTextureStatus.Force32:
                throw new Exception($"What is going on bros... {status}");
        }

        using var surfaceTextureView = surfaceTexture.CreateView();

        var colorAttachments = stackalloc RenderPassColorAttachment[1];
        colorAttachments[0] = new RenderPassColorAttachment
        {
            ClearValue = new (100 / 255.0, 149 / 255.0, 237 / 255.0, 1),
            LoadOp = LoadOp.Clear,
            StoreOp = StoreOp.Store,
            View = surfaceTextureView,
        };

        var renderPassDesc = new RenderPassDescriptor
        {
            ColorAttachmentCount = 1,
            ColorAttachments = colorAttachments,
        };

        using var commandEncoder = device.CreateCommandEncoder();
        using var renderPassEncoder = commandEncoder.BeginRenderPass(renderPassDesc);
        renderPassEncoder.End();
        using var commandBuffer = commandEncoder.Finish();
        queue.Submit(1, commandBuffer);
        surface.Present();
        window.SwapBuffers();
    }
}
