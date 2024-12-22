using System.Numerics;
using System.Runtime.CompilerServices;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

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

    private IWindow window = default!;
    private IInputContext? input;
    private IKeyboard? keyboard;
    private ShaderModule? shader;
    private RenderPipeline? renderPipeline;

    private Buffer? vertexBuffer;
    private ulong vertexBufferSize;

    private Texture? texture;
    private TextureView? textureView;
    private Sampler? sampler;

    private BindGroup? textureBindGroup;
    private BindGroupLayout? textureSamplerBindGroupLayout;

    private Buffer? projectionMatrixBuffer;
    private BindGroupLayout? projectionMatrixBindGroupLayout;
    private BindGroup? projectionMatrixBindGroup;

    public GraphicsDeviceManager Graphics { get; set; }

    public Demo()
    {
        var options = WebGPUWindowOptions.Default with
        {
            Size = new Vector2D<int>(800, 600),
            Title = "WebGPU with Silk.NET",
        };

        window = Window.Create(options);
        Graphics = new GraphicsDeviceManager(window);
        
        //Assign events.
        window.Load += OnLoad;
        window.Update += OnUpdate;
        window.Render += OnRender;
        window.Closing += OnClosing;
        window.FramebufferResize += FramebufferResize;

    }

    internal void Run()
    {
        //Run the window.
        window.Run();
    }

    private void OnClosing()
    {
        Dispose();
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
        Graphics?.Dispose();
        input?.Dispose();
        input = null;
        window?.Dispose();
        window = null!;
    }

    private void FramebufferResize(Vector2D<int> size)
    {
        Graphics.CreateSwapChain();
        UpdateProjectionMatrix();
    }

    private void OnLoad()
    {
        input = window.CreateInput();
        keyboard = input.Keyboards[0];

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

        shader = Graphics.Device.CreateShaderModuleWGSL(shaderCode);

        texture = Graphics.Device.LoadTexture("silk.png", TextureFormat.Rgba8Unorm);
        textureView = texture.CreateView();

        sampler = Graphics.Device.CreateSampler(FilterMode.Linear, MipmapFilterMode.Linear);

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

            textureSamplerBindGroupLayout = Graphics.Device.CreateBindGroupLayout(in layoutDescriptor);

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

            textureBindGroup = Graphics.Device.CreateBindGroup(in descriptor);

        } //Create bind group for sampler and texture view

        projectionMatrixBuffer = Graphics.Device.CreateBuffer<Matrix4x4>(BufferUsage.Uniform | BufferUsage.CopyDst);
        UpdateProjectionMatrix();

        { //Create bind group for projection matrix
            var entry = new BindGroupLayoutEntry
            {
                Binding = 0,
                Buffer = new BufferBindingLayout
                {
                    Type = BufferBindingType.Uniform,
                    MinBindingSize = (ulong)Unsafe.SizeOf<Matrix4x4>()
                },
                Visibility = ShaderStage.Vertex,
            };

            var projectionMatrixLayoutDescriptor = new BindGroupLayoutDescriptor
            {
                Entries = [entry]
            };

            projectionMatrixBindGroupLayout = Graphics.Device.CreateBindGroupLayout(in projectionMatrixLayoutDescriptor);

            var bindGroupEntry = new BindGroupEntry
            {
                Binding = 0,
                Buffer = projectionMatrixBuffer,
                Size = (ulong)Unsafe.SizeOf<Matrix4x4>()
            };

            BindGroupDescriptor projectionMatrixBindGroupDescriptor = new BindGroupDescriptor
            {
                Entries = [bindGroupEntry],
                Layout = projectionMatrixBindGroupLayout
            };
            projectionMatrixBindGroup = Graphics.Device.CreateBindGroup(in projectionMatrixBindGroupDescriptor);
        } //Create bind group for projection matrix 

        { //Create vertex buffer

            vertexBuffer = Graphics.Device.CreateBuffer<Vertex>(BufferUsage.Vertex | BufferUsage.CopyDst, 6);
            vertexBufferSize = vertexBuffer.GetSize();

            //Get a queue
            using var queue = Graphics.Device.GetQueue();

            const float xPos = 100;
            const float yPos = 100;
            const float width = 271;
            const float height = 271;

            //Fill data with a quad with a CCW front face
            ReadOnlySpan<Vertex> data =
            [
                new Vertex(new Vector2(xPos, yPos), new Vector2(0, 0)), //Top left
                    new Vertex(new Vector2(xPos + width, yPos), new Vector2(1, 0)),  //Top right
                    new Vertex(new Vector2(xPos + width, yPos + height), new Vector2(1, 1)),   //Bottom right
                    new Vertex(new Vector2(xPos, yPos), new Vector2(0, 0)), //Top left
                    new Vertex(new Vector2(xPos + width, yPos + height), new Vector2(1, 1)),   //Bottom right
                    new Vertex(new Vector2(xPos, yPos + height), new Vector2(0, 1)),  //Bottom left
                ];

            //Write the data to the buffer
            queue.WriteBuffer(vertexBuffer, data);
        } //Create vertex buffer


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
                    Offset = (ulong)Unsafe.SizeOf<Vector2>(),
                    ShaderLocation = 1
                }
            ],
            StepMode = VertexStepMode.Vertex,
            ArrayStride = (ulong)Unsafe.SizeOf<Vertex>()
        };

        var colorTargetState = new ColorTargetState
        {
            Format = Graphics.DefaultSurfaceFormat,
            Blend = BlendStates.NonPremultiplied,
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

        using var pipelineLayout = Graphics.Device!.CreatePipelineLayout(in pipelineLayoutDescriptor);

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

        renderPipeline = Graphics.Device.CreateRenderPipeline(in renderPipelineDescriptor);
    }

    private unsafe void UpdateProjectionMatrix()
    {
        using var queue = Graphics.Device!.GetQueue();

        var projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0, window!.Size.X, window.Size.Y, 0, 0, 1);

        queue.WriteBuffer(projectionMatrixBuffer, projectionMatrix);
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
        using var surfaceTextureView = Graphics.GetCurrentSurfaceTextureView();
        if (surfaceTextureView is null) return;

        var renderPassDesc = new RenderPassDescriptor
        {
            ColorAttachments =
            [
                new RenderPassColorAttachment
                {
                    ClearValue = new(0, 0, 0, 1),
                    //DepthSlice = 0,
                    LoadOp = LoadOp.Clear,
                    StoreOp = StoreOp.Store,
                    View = surfaceTextureView,
                    ResolveTarget = null,
                }
            ],
        };

        using var commandEncoder = Graphics.Device!.CreateCommandEncoder();

        using var renderPassEncoder = commandEncoder.BeginRenderPass(in renderPassDesc);
        renderPassEncoder.SetPipeline(renderPipeline);
        renderPassEncoder.SetBindGroup(0, textureBindGroup);
        renderPassEncoder.SetBindGroup(1, projectionMatrixBindGroup);
        renderPassEncoder.SetVertexBuffer(0, vertexBuffer, 0, vertexBufferSize);
        renderPassEncoder.Draw(6, 1, 0, 0);
        renderPassEncoder.End();
        using var commandBuffer = commandEncoder.Finish();
        Graphics.Queue!.Submit(commandBuffer);
        Graphics.Surface.Present();
        window!.SwapBuffers();
    }
}
