using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.Windowing;
using System.Numerics;
using SixLabors.ImageSharp;
using Silk.NET.WebGPU.Extensions.WGPU;
using SixLabors.ImageSharp.PixelFormats;
using Silk.NET.Core.Native;
using System.Runtime.InteropServices;
using Silk.NET.Core.Attributes;
using System.Runtime.CompilerServices;

namespace SilkWrapped.WebGPU.Example;
internal class Demo : IDisposable
{
    public struct Vertex
    {
        public Vertex(Vector2 position, Vector2 texCoord)
        {
            Position = position;
            TexCoord = texCoord;
        }

        public Vector2 Position;
        public Vector2 TexCoord;
    }

    private IWindow? window;
    private IInputContext? input;
    private IKeyboard? keyboard;
    private InstanceWrapper? instance;
    private SurfaceWrapper? surface;
    private SurfaceCapabilities surfaceCapabilities;
    private AdapterWrapper? adapter;
    private DeviceWrapper? device;

    private QueueWrapper? queue;
    private SurfaceConfiguration surfaceConfiguration;
    private ShaderModuleWrapper? shader;
    private RenderPipelineWrapper? renderPipeline;

    private BufferWrapper? vertexBuffer;
    private ulong vertexBufferSize;

    private TextureWrapper? texture;
    private TextureViewWrapper? textureView;
    private SamplerWrapper? sampler;

    private BindGroupWrapper? textureBindGroup;
    private BindGroupLayoutWrapper? textureSamplerBindGroupLayout;

    private BufferWrapper? projectionMatrixBuffer;
    private BindGroupLayoutWrapper? projectionMatrixBindGroupLayout;
    private BindGroupWrapper? projectionMatrixBindGroup;


    public Demo()
    {

    }
    public void Dispose()
    {
        renderPipeline?.Dispose();

        projectionMatrixBindGroup?.Dispose();
        projectionMatrixBindGroupLayout?.Dispose();
        projectionMatrixBuffer?.Dispose();

        vertexBuffer?.Dispose();
        textureBindGroup?.Dispose();
        textureSamplerBindGroupLayout?.Dispose();
        textureView?.Dispose();
        texture?.Dispose();
        sampler?.Dispose();
        shader?.Dispose();
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
        window.FramebufferResize += FramebufferResize;

        //Run the window.
        window.Run();
    }

    private void FramebufferResize(Vector2D<int> obj)
    {
        CreateSwapChain();
        UpdateProjectionMatrix();
    }


    unsafe static void DV(DeviceLostReason reason, byte* message, void* userdata)
    {

    }

    unsafe static void EC(ErrorType reason, byte* message, void* userdata)
    {
        var x = Marshal.PtrToStringAnsi((nint)message);
    }

    unsafe static DeviceDescriptor d = new DeviceDescriptor
    {
        DeviceLostCallback = new PfnDeviceLostCallback(DV)
    };
    private TextureFormat[] surfaceFormats;

    private void OnLoad()
    {
        input = window!.CreateInput();
        keyboard = input.Keyboards[0];

        instance = new InstanceWrapper();
        surface = window!.CreateWebGPUSurface(instance);

        adapter = instance.RequestAdapter(surface);


        device = adapter.RequestDevice(in d);


        surface.GetCapabilities(adapter, ref surfaceCapabilities);
        queue = device.GetQueue();

        CreateSwapChain();

        var shaderCode =
            """
            struct VertexOutputs {
                //The position of the vertex
                @builtin(position) position: vec4<f32>,
                //The texture cooridnate of the vertex
                @location(0) tex_coord: vec2<f32>
            }

            @group(1) @binding(0) var<uniform> projection_matrix: mat4x4<f32>;

            @vertex
            fn vs_main(
                @location(0) pos: vec2<f32>,
                @location(1) tex_coord: vec2<f32>
            ) -> VertexOutputs {
                var output: VertexOutputs;

                output.position = projection_matrix * vec4<f32>(pos, 0.0, 1.0);
                output.tex_coord = tex_coord;

                return output;
            }

            //The texture we're sampling
            @group(0) @binding(0) var t: texture_2d<f32>;
            //The sampler we're using to sample the texture
            @group(0) @binding(1) var s: sampler;

            @fragment
            fn fs_main(input: VertexOutputs) -> @location(0) vec4<f32> {
                return textureSample(t, s, input.tex_coord);
            }
            """;

        shader = device.CreateShaderModuleWGSL(shaderCode);

        unsafe
        {
            surfaceFormats = new Span<TextureFormat>(surfaceCapabilities.Formats, (int)surfaceCapabilities.FormatCount).ToArray();

            var dummy = 0;
            device.SetUncapturedErrorCallback(EC, ref dummy);


            { //Create texture and texture view
                using var image = Image.Load<Rgba32>("silk.png");
                
                var viewFormat = TextureFormat.Rgba8Unorm;

                var descriptor = new TextureDescriptor
                {
                    Size = new Extent3D((uint)image.Width, (uint)image.Height, 1),
                    Format = TextureFormat.Rgba8Unorm,
                    Usage = TextureUsage.CopyDst | TextureUsage.TextureBinding,
                    MipLevelCount = 1,
                    SampleCount = 1,
                    Dimension = TextureDimension.Dimension2D,
                    ViewFormats = &viewFormat,
                    ViewFormatCount = 1
                };

                texture = device.CreateTexture(in descriptor);

                var viewDescriptor = new TextureViewDescriptor()
                {
                    Format = TextureFormat.Rgba8Unorm,
                    Dimension = TextureViewDimension.Dimension2D,
                    Aspect = TextureAspect.All,
                    MipLevelCount = 1,
                    ArrayLayerCount = 1,
                    BaseArrayLayer = 0,
                    BaseMipLevel = 0
                };

                textureView = texture.CreateView(in viewDescriptor);

                using var queue = device.GetQueue();

                using var commandEncoder = device.CreateCommandEncoder();

                var layout = new TextureDataLayout
                {
                    BytesPerRow = (uint)(image.Width * sizeof(Rgba32)),
                    RowsPerImage = (uint)image.Height
                };
                // layout.Offset = layout.BytesPerRow * (uint) i;

                var extent = new Extent3D
                {
                    Width = (uint)image.Width,
                    Height = 1,
                    DepthOrArrayLayers = 1
                };

                image.ProcessPixelRows
                (
                    x =>
                    {
                        for (var i = 0; i < x.Height; i++)
                        {
                            var imageRow = x.GetRowSpan(i);
                            
                            var imageCopyTexture = new ImageCopyTexture
                            {
                                Texture = texture,
                                Aspect = TextureAspect.All,
                                MipLevel = 0,
                                Origin = new Origin3D(0, (uint)i, 0)
                            };
                            //fixed (void* dataPtr = imageRow)
                            queue.WriteTexture(in imageCopyTexture, in imageRow.GetPinnableReference(), (nuint)(sizeof(Rgba32) * imageRow.Length), in layout, in extent);
                        }
                    }
                );

                using var commandBuffer = commandEncoder.Finish();

                queue.Submit(1, commandBuffer);
            } //Create texture and texture view




            { //Create sampler
                var descriptor = new SamplerDescriptor
                {
                    Compare = CompareFunction.Undefined,
                    MipmapFilter = MipmapFilterMode.Linear,
                    MagFilter = FilterMode.Linear,
                    MinFilter = FilterMode.Linear,
                    MaxAnisotropy = 1
                };

                sampler = device.CreateSampler(in descriptor);
            } //Create sampler

            { //Create bind group for sampler and textureview
                var entries = stackalloc BindGroupLayoutEntry[2];
                entries[0] = new BindGroupLayoutEntry
                {
                    Binding = 0,
                    Texture = new TextureBindingLayout
                    {
                        Multisampled = false,
                        SampleType = TextureSampleType.Float,
                        ViewDimension = TextureViewDimension.Dimension2D
                    },
                    Visibility = ShaderStage.Fragment
                };
                entries[1] = new BindGroupLayoutEntry
                {
                    Binding = 1,
                    Sampler = new SamplerBindingLayout
                    {
                        Type = SamplerBindingType.Filtering
                    },
                    Visibility = ShaderStage.Fragment
                };

                var layoutDescriptor = new BindGroupLayoutDescriptor
                {
                    Entries = entries,
                    EntryCount = 2
                };

                textureSamplerBindGroupLayout = device.CreateBindGroupLayout(in layoutDescriptor);

                var bindGroupEntries = stackalloc BindGroupEntry[2];
                bindGroupEntries[0] = new BindGroupEntry
                {
                    Binding = 0,
                    TextureView = textureView
                };
                bindGroupEntries[1] = new BindGroupEntry
                {
                    Binding = 1,
                    Sampler = sampler
                };

                var descriptor = new BindGroupDescriptor
                {
                    Entries = bindGroupEntries,
                    EntryCount = 2,
                    Layout = textureSamplerBindGroupLayout
                };

                textureBindGroup = device.CreateBindGroup(in descriptor);
                
            } //Create bind group for sampler and texture view

            { //Create buffer to store projection matrix
                var descriptor = new BufferDescriptor
                {
                    Size = (ulong)sizeof(Matrix4x4),
                    Usage = BufferUsage.Uniform | BufferUsage.CopyDst,
                    MappedAtCreation = false
                };

                projectionMatrixBuffer = device.CreateBuffer(in descriptor);
                UpdateProjectionMatrix();
            } //Create buffer to store projection matrix

            { //Create bind group for projection matrix
                var entry = new BindGroupLayoutEntry
                {
                    Binding = 0,
                    Buffer = new BufferBindingLayout
                    {
                        Type = BufferBindingType.Uniform,
                        MinBindingSize = (ulong)sizeof(Matrix4x4)
                    },
                    Visibility = ShaderStage.Vertex,
                };

                var projectionMatrixLayoutDescriptor = new BindGroupLayoutDescriptor
                {
                    Entries = &entry,
                    EntryCount = 1
                };

                projectionMatrixBindGroupLayout = device.CreateBindGroupLayout(in projectionMatrixLayoutDescriptor);

                var bindGroupEntry = new BindGroupEntry
                {
                    Binding = 0,
                    Buffer = projectionMatrixBuffer,
                    Size = (ulong)sizeof(Matrix4x4)
                };

                BindGroupDescriptor projectionMatrixBindGroupDescriptor = new BindGroupDescriptor
                {
                    Entries = &bindGroupEntry,
                    EntryCount = 1,
                    Layout = projectionMatrixBindGroupLayout
                };
                projectionMatrixBindGroup = device.CreateBindGroup(in projectionMatrixBindGroupDescriptor);
            } //Create bind group for projection matrix 

            { //Create vertex buffer
                var descriptor = new BufferDescriptor
                {
                    Size = vertexBufferSize = (ulong)(sizeof(Vertex) * 6),
                    Usage = BufferUsage.Vertex | BufferUsage.CopyDst
                };

                vertexBuffer = device.CreateBuffer(in descriptor);

                //Get a queue
                using var queue = device.GetQueue();

                var data = stackalloc Vertex[6];

                const float xPos = 100;
                const float yPos = 100;
                const float width = 271;
                const float height = 271;

                //Fill data with a quad with a CCW front face
                data[0] = new Vertex(new Vector2(xPos, yPos), new Vector2(0, 0)); //Top left
                data[1] = new Vertex(new Vector2(xPos + width, yPos), new Vector2(1, 0));  //Top right
                data[2] = new Vertex(new Vector2(xPos + width, yPos + height), new Vector2(1, 1));   //Bottom right
                data[3] = new Vertex(new Vector2(xPos, yPos), new Vector2(0, 0)); //Top left
                data[4] = new Vertex(new Vector2(xPos + width, yPos + height), new Vector2(1, 1));   //Bottom right
                data[5] = new Vertex(new Vector2(xPos, yPos + height), new Vector2(0, 1));  //Bottom left

                //Write the data to the buffer
                queue.WriteBuffer(vertexBuffer, 0, in data[0], (nuint)vertexBufferSize);

                ////Create a new command encoder
                //using var commandEncoder = device.CreateCommandEncoder();

                ////Finish the command encoder
                //using var commandBuffer = commandEncoder.Finish();

                //queue.Submit(1, commandBuffer);
            } //Create vertex buffer
        }


        CreateRenderPipeline();
    }

    private unsafe void CreateRenderPipeline()
    {
        var vertexAttributes = stackalloc VertexAttribute[2];

        vertexAttributes[0] = new VertexAttribute
        {
            Format = VertexFormat.Float32x2,
            Offset = 0,
            ShaderLocation = 0
        };
        vertexAttributes[1] = new VertexAttribute
        {
            Format = VertexFormat.Float32x2,
            Offset = (ulong)sizeof(Vector2),
            ShaderLocation = 1
        };

            var vertexBufferLayout = new VertexBufferLayout
            {
            Attributes = vertexAttributes,
            AttributeCount = 2,
                StepMode = VertexStepMode.Vertex,
            ArrayStride = (ulong)sizeof(Vertex)
            };



        fixed (byte* fs_main = ("fs_main"u8))
        fixed(byte* vs_main = ("vs_main"u8))
        {
            var blendState = new BlendState
            {
                Color = new BlendComponent
                {
                    SrcFactor = BlendFactor.SrcAlpha,
                    DstFactor = BlendFactor.OneMinusSrcAlpha,
                    Operation = BlendOperation.Add
                },
                Alpha = new BlendComponent
                {
                    SrcFactor = BlendFactor.One,
                    DstFactor = BlendFactor.OneMinusSrcAlpha,
                    Operation = BlendOperation.Add
                }
            };

            var colorTargetState = new ColorTargetState
            {
                Format = surfaceCapabilities.Formats[0],
                Blend = &blendState,
                WriteMask = ColorWriteMask.All
            };

            var fragmentState = new FragmentState
            {
                Module = shader,
                TargetCount = 1,
                Targets = &colorTargetState,
                EntryPoint = fs_main
            };

            var bindGroupLayouts = stackalloc BindGroupLayout*[2];
            bindGroupLayouts[0] = textureSamplerBindGroupLayout;
            bindGroupLayouts[1] = projectionMatrixBindGroupLayout;
            
            var pipelineLayoutDescriptor = new PipelineLayoutDescriptor
            {
                BindGroupLayoutCount = 2,
                BindGroupLayouts = bindGroupLayouts
            };

            using var pipelineLayout = device!.CreatePipelineLayout(in pipelineLayoutDescriptor);

            var renderPipelineDescriptor = new RenderPipelineDescriptor
            {
                Vertex = new VertexState
                {
                    Module = shader,
                    EntryPoint = vs_main,
                    Buffers = &vertexBufferLayout,
                    BufferCount = 1
                },
                Primitive = new PrimitiveState
                {
                    Topology = PrimitiveTopology.TriangleList,
                    StripIndexFormat = IndexFormat.Undefined,
                    FrontFace = FrontFace.Ccw,
                    CullMode = CullMode.None
                },
                Multisample = new MultisampleState
                {
                    Count = 1,
                    Mask = ~0u,
                    AlphaToCoverageEnabled = false
                },
                Fragment = &fragmentState,
                DepthStencil = null,
                Layout = pipelineLayout
            };

            renderPipeline = device.CreateRenderPipeline(in renderPipelineDescriptor);
        }
    }

    private unsafe void CreateSwapChain()
    {
        surfaceConfiguration = new SurfaceConfiguration
        {
            Usage = TextureUsage.RenderAttachment,
            Format = surfaceCapabilities.Formats[0],
            PresentMode = PresentMode.Fifo,
            Device = device,
            Width = (uint)window!.FramebufferSize.X,
            Height = (uint)window.FramebufferSize.Y
        };

        surface!.Configure(in surfaceConfiguration);
    }

    private unsafe void UpdateProjectionMatrix()
    {
        using var queue = device!.GetQueue();

        using var commandEncoder = device.CreateCommandEncoder();
        var projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0, window!.Size.X, window.Size.Y, 0, 0, 1);

        queue.WriteBuffer(projectionMatrixBuffer, 0, in projectionMatrix, (nuint)sizeof(Matrix4x4));

        using var commandBuffer = commandEncoder.Finish();

        queue.Submit(1, commandBuffer);
    }

    private void OnUpdate(double obj)
    {
        if (keyboard!.IsKeyPressed(Key.Escape))
        {
            window!.Close();
        }
    }

    private unsafe void OnRender(double obj)
    {
        var (status, surfaceTexture) = surface!.GetCurrentTexture();
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
            ClearValue = new(1, 1, 1, 1),
            //DepthSlice = 0,
            LoadOp = LoadOp.Clear,
            StoreOp = StoreOp.Store,
            View = surfaceTextureView,
            ResolveTarget = null,
        };

        var renderPassDesc = new RenderPassDescriptor
        {
            ColorAttachmentCount = 1,
            ColorAttachments = colorAttachments,
        };

        using var commandEncoder = device!.CreateCommandEncoder();

        using var renderPassEncoder = commandEncoder.BeginRenderPass(in renderPassDesc);
        renderPassEncoder.SetPipeline(renderPipeline);
        uint zero = 0;
        renderPassEncoder.SetBindGroup(0, textureBindGroup, 0, in zero);
        renderPassEncoder.SetBindGroup(1, projectionMatrixBindGroup, 0, in zero);
        renderPassEncoder.SetVertexBuffer(0, vertexBuffer, 0, vertexBufferSize);
        renderPassEncoder.Draw(6, 1, 0, 0);
        renderPassEncoder.End();
        using var commandBuffer = commandEncoder.Finish();
        queue!.Submit(1, commandBuffer);
        surface.Present();
        window!.SwapBuffers();
    }
}
