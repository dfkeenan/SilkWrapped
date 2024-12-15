using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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
    private AdapterWrapper? adapter;
    private DeviceWrapper? device;

    private QueueWrapper? queue;
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

    private static unsafe void DV(DeviceLostReason reason, string? message, void* userdata)
    {

    }

    private static unsafe void EC(ErrorType reason, string? message, void* userdata)
    {

    }


    private TextureFormat[] surfaceFormats;

    private void OnLoad()
    {
        input = window!.CreateInput();
        keyboard = input.Keyboards[0];

        instance = new InstanceWrapper();
        surface = window!.CreateWebGPUSurface(instance);

        adapter = instance.RequestAdapter(surface);


        device = adapter.RequestDevice();

        SurfaceCapabilities surfaceCapabilities = default;
        surface.GetCapabilities(adapter, ref surfaceCapabilities);
        surfaceFormats = surfaceCapabilities.Formats;


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
            device.SetUncapturedErrorCallback(EC);


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
                    ViewFormats = [viewFormat],
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

                queue.Submit([commandBuffer]);
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
                var layoutDescriptor = new BindGroupLayoutDescriptor
                {
                    Entries =
                    [
                        new BindGroupLayoutEntry
                        {
                            Binding = 0,
                            Texture = new TextureBindingLayout
                            {
                                Multisampled = false,
                                SampleType = TextureSampleType.Float,
                                ViewDimension = TextureViewDimension.Dimension2D
                            },
                            Visibility = ShaderStage.Fragment
                        },
                        new BindGroupLayoutEntry
                        {
                            Binding = 1,
                            Sampler = new SamplerBindingLayout
                            {
                                Type = SamplerBindingType.Filtering
                            },
                            Visibility = ShaderStage.Fragment
                        }
                    ]
                };

                textureSamplerBindGroupLayout = device.CreateBindGroupLayout(in layoutDescriptor);

                var descriptor = new BindGroupDescriptor
                {
                    Entries = [new BindGroupEntry
                                {
                                    Binding = 0,
                                    TextureView = textureView
                                },
                                new BindGroupEntry
                                {
                                    Binding = 1,
                                    Sampler = sampler
                                }],
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
                    Entries = [entry]
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
                    Entries = [bindGroupEntry],
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

                //Create a new command encoder
                using var commandEncoder = device.CreateCommandEncoder();

                //Finish the command encoder
                using var commandBuffer = commandEncoder.Finish();

                queue.Submit([commandBuffer]);
            } //Create vertex buffer
        }


        CreateRenderPipeline();
    }

    private unsafe void CreateRenderPipeline()
    {
        var vertexBufferLayout = new VertexBufferLayout
        {
            Attributes =
            [
                new VertexAttribute
                {
                    Format = VertexFormat.Float32x2,
                    Offset = 0,
                    ShaderLocation = 0
                },
                new VertexAttribute
                {
                    Format = VertexFormat.Float32x2,
                    Offset = (ulong)sizeof(Vector2),
                    ShaderLocation = 1
                }
            ],
            StepMode = VertexStepMode.Vertex,
            ArrayStride = (ulong)sizeof(Vertex)
        };

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
            Format = surfaceFormats[0],
            Blend = blendState,
            WriteMask = ColorWriteMask.All
        };

        var fragmentState = new FragmentState
        {
            Module = shader,
            Targets = [colorTargetState],
            EntryPoint = "fs_main"
        };

        var pipelineLayoutDescriptor = new PipelineLayoutDescriptor
        {
            BindGroupLayouts =
            [
                textureSamplerBindGroupLayout,
                projectionMatrixBindGroupLayout
            ]
        };

        using var pipelineLayout = device!.CreatePipelineLayout(in pipelineLayoutDescriptor);

        var renderPipelineDescriptor = new RenderPipelineDescriptor
        {
            Vertex = new VertexState
            {
                Module = shader,
                EntryPoint = "vs_main",
                Buffers = [vertexBufferLayout],
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
            Fragment = fragmentState,
            DepthStencil = null,
            Layout = pipelineLayout
        };

        renderPipeline = device.CreateRenderPipeline(in renderPipelineDescriptor);
    }

    private unsafe void CreateSwapChain()
    {
        var surfaceConfiguration = new SurfaceConfiguration
        {
            Usage = TextureUsage.RenderAttachment,
            Format = surfaceFormats[0],
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

        queue.Submit([commandBuffer]);
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

        var renderPassDesc = new RenderPassDescriptor
        {
            ColorAttachments =
            [
                new RenderPassColorAttachment
                {
                    ClearValue = new(1, 1, 1, 1),
                    //DepthSlice = 0,
                    LoadOp = LoadOp.Clear,
                    StoreOp = StoreOp.Store,
                    View = surfaceTextureView,
                    ResolveTarget = null,
                }
            ],
        };

        using var commandEncoder = device!.CreateCommandEncoder();

        using var renderPassEncoder = commandEncoder.BeginRenderPass(in renderPassDesc);
        renderPassEncoder.SetPipeline(renderPipeline);
        renderPassEncoder.SetBindGroup(0, textureBindGroup, []);
        renderPassEncoder.SetBindGroup(1, projectionMatrixBindGroup, []);
        renderPassEncoder.SetVertexBuffer(0, vertexBuffer, 0, vertexBufferSize);
        renderPassEncoder.Draw(6, 1, 0, 0);
        renderPassEncoder.End();
        using var commandBuffer = commandEncoder.Finish();
        queue!.Submit([commandBuffer]);
        surface.Present();
        window!.SwapBuffers();
    }
}
