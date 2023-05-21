using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.Windowing;

namespace SilkWrapped.WebGPU.Example;
internal unsafe class Demo : IDisposable
{
    private IWindow window = default!;
    private IInputContext input = default!;
    private IKeyboard keyboard;
    private InstanceWrapper instance;
    private SurfaceWrapper surface;
    private AdapterWrapper adapter;
    private DeviceWrapper device;
    private SwapChainWrapper swapChain;
    private QueueWrapper queue;
    private TextureFormat swapChainFormat;

    public Demo()
    {
            
    }
    public void Dispose()
    {
        queue?.Dispose();
        swapChain?.Dispose();
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

    private void OnLoad()
    {
        input = window.CreateInput();
        keyboard = input.Keyboards[0];

        instance = new InstanceWrapper(new InstanceDescriptor());
        surface = window.CreateWebGPUSurface(instance);
        int dummy = 69;
        
        instance.RequestAdapter(new RequestAdapterOptions() { CompatibleSurface = surface },
            new PfnRequestAdapterCallback((arg0, arg1, arg2, arg3) =>
            {
                if (arg0 == RequestAdapterStatus.Success)
                {
                    adapter = new AdapterWrapper(instance.Api, arg1);
                }
            }),
            ref dummy);

        if (adapter != null)
        {
            var dd = new DeviceDescriptor
            {
                
            };

            adapter.RequestDevice(dd,
                new PfnRequestDeviceCallback((arg0, arg1, arg2, arg3) =>
                {
                    if (arg0 == RequestDeviceStatus.Success)
                    {
                        device = new DeviceWrapper(instance.Api, arg1);
                    }
                }), ref dummy);
        }
        queue = device.GetQueue();
        CreateSwapChain();
    }

    private void CreateSwapChain()
    {
        if (device != null)
        {
            swapChain?.Dispose();
            swapChainFormat = surface.GetPreferredFormat(adapter);
            var scd = new SwapChainDescriptor
            {
                Usage = TextureUsage.RenderAttachment,
                Format = swapChainFormat,
                Width = (uint)window.Size.X,
                Height = (uint)window.Size.Y,
                PresentMode = PresentMode.Fifo,
            };
            swapChain = device.CreateSwapChain(surface, scd);
        }
    }

    private void OnUpdate(double obj)
    {
        if(keyboard.IsKeyPressed(Key.Escape))
        {
            window.Close();
        }
    }

    private void OnRender(double obj)
    {
        using var swapChainView = swapChain.GetCurrentTextureView();

        var colorAttachments = stackalloc RenderPassColorAttachment[1];
        colorAttachments[0] = new RenderPassColorAttachment
        {
            ClearValue = new Color(100 / 255.0, 149 / 255.0, 237 / 255.0, 1),
            LoadOp = LoadOp.Clear,
            StoreOp = StoreOp.Store,
            View = swapChainView,
        };

        var renderPassDesc = new RenderPassDescriptor
        {
            ColorAttachmentCount = 1,
            ColorAttachments = colorAttachments,
        };

        using var commandEncoder = device.CreateCommandEncoder(new CommandEncoderDescriptor { });
        using var renderPassEncoder = commandEncoder.BeginRenderPass(renderPassDesc);
        renderPassEncoder.End();
        using var commandBuffer = commandEncoder.Finish(new CommandBufferDescriptor { });
        var cbHandle = commandBuffer.Handle;
        queue.Submit(1, ref cbHandle);

        swapChain.Present();
        window.SwapBuffers();
    }
}
