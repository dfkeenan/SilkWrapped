using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace SilkWrapped.WebGPU.Example;

[VertexStruct]
[StructLayout(LayoutKind.Sequential)]
internal readonly partial record struct Vertex(Vector2 Position, Vector2 TexCoord);

[BindGroup]
internal partial class ProjectionMatrixBindGroup(Device device)
{
    [UniformBinding(ShaderStage.Vertex)]
    public partial Matrix4x4 Projection { get; set; }
}

[BindGroup]
internal partial class TextureBindGroup(
     Device device,
     [TextureBinding(TextureSampleType.Float, TextureViewDimension.Dimension2D, ShaderStage.Fragment)] TextureView textureView,
     [SamplerBinding(SamplerBindingType.Filtering, ShaderStage.Fragment)] Sampler sampler);

internal class Demo : IDisposable
{
    private IWindow window = default!;
    private IInputContext? input;
    private IKeyboard? keyboard;
    private ShaderModule? shader;
    private RenderPipeline? renderPipeline;

    private Buffer<Vertex>? vertexBuffer;

    private Texture? texture;
    private TextureView? textureView;
    private Sampler? sampler;

    private TextureBindGroup? textureBindGroup;
    private ProjectionMatrixBindGroup? projectionMatrixBindGroup;

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

        vertexBuffer?.Dispose();
        textureBindGroup?.Dispose();
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
        Graphics.ResizeSwapChain();
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

        textureBindGroup = new TextureBindGroup(Graphics.Device, textureView, sampler);
        projectionMatrixBindGroup = new ProjectionMatrixBindGroup(Graphics.Device);
        UpdateProjectionMatrix();

        { //Create vertex buffer

            vertexBuffer = Graphics.Device.CreateBuffer<Vertex>(BufferUsage.Vertex | BufferUsage.CopyDst, 6);

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
        using var pipelineLayout = Graphics.Device!.CreatePipelineLayout(
            textureBindGroup,
            projectionMatrixBindGroup);

        var renderPipelineDescriptor = new RenderPipelineDescriptor
        {
            Layout = pipelineLayout,
            Vertex = new VertexState
            {
                Module = shader!,
                EntryPoint = "vs_main",
                Buffers = [Vertex.GetLayout()],
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
            Fragment = new FragmentState
            {
                Module = shader!,
                EntryPoint = "fs_main",
                Targets =
                [
                    new ColorTargetState
                    {
                        Format = Graphics.DefaultSurfaceFormat,
                        Blend = BlendStates.NonPremultiplied,
                        WriteMask = ColorWriteMask.All
                    }
                ]
            },
            DepthStencil = null,
        };

        renderPipeline = Graphics.Device.CreateRenderPipeline(in renderPipelineDescriptor);
    }

    private unsafe void UpdateProjectionMatrix()
    {
        projectionMatrixBindGroup!.Projection
            = Matrix4x4.CreateOrthographicOffCenter(0, window!.Size.X, window.Size.Y, 0, 0, 1);
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
        if (renderPipeline is null || vertexBuffer is null) return;

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
        renderPassEncoder.SetBindGroup(0, textureBindGroup!);
        renderPassEncoder.SetBindGroup(1, projectionMatrixBindGroup!);
        renderPassEncoder.SetVertexBuffer(0, vertexBuffer, 0, vertexBuffer.Size);
        renderPassEncoder.Draw(6, 1, 0, 0);
        renderPassEncoder.End();
        using var commandBuffer = commandEncoder.Finish();
        Graphics.Queue!.Submit(commandBuffer);
        Graphics.Surface.Present();
        window!.SwapBuffers();
    }
}
