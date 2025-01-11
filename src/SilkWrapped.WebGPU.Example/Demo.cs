using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace SilkWrapped.WebGPU.Example;

[VertexStruct]
[StructLayout(LayoutKind.Sequential)]
internal readonly partial record struct Vertex(Vector3 Position, Vector2 TexCoord, Vector3 Normal, Vector4 Color);

[BindGroup]
internal partial class CameraBindGroup(Device device)
{
    [UniformBinding(ShaderStage.Vertex)]
    public partial Matrix4x4 View { get; set; }

    [UniformBinding(ShaderStage.Vertex)]
    public partial Matrix4x4 Projection { get; set; }
}

[BindGroup]
internal partial class ModelBindGroup(
     Device device,
     [TextureBinding(TextureSampleType.Float, TextureViewDimension.Dimension2D, ShaderStage.Fragment)] TextureView textureView,
     [SamplerBinding(SamplerBindingType.Filtering, ShaderStage.Fragment)] Sampler sampler)
{
    [UniformBinding(ShaderStage.Vertex)]
    public partial Matrix4x4 World { get; set; }
}

internal class Demo : IDisposable
{
    private IWindow window = default!;
    private IInputContext? input;
    private IKeyboard? keyboard;
    private ShaderModule? shader;
    private RenderPipeline? renderPipeline;

    private MeshData<Vertex> cube = Shapes.Cube(new Vector3(3, 3, 3));
    private Buffer<Vertex>? vertexBuffer;
    private Buffer<uint>? indexBuffer;

    private Texture? texture;
    private TextureView? textureView;
    private Sampler? sampler;

    private ModelBindGroup? modelBindGroup;
    private CameraBindGroup? cameraBindGroup;

    private Texture? depthTexture;
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

        Graphics.DeviceLost += (r, m) =>
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{r} - {m}");
            Console.ResetColor();
        };

        Graphics.UncapturedError += (r, m) =>
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{r} - {m}");
            Console.ResetColor();
        };

        //Assign events.
        window.Load += OnLoad;
        window.Update += OnUpdate;
        window.Render += OnRender;
        window.FramebufferResize += FramebufferResize;

    }

    internal void Run()
    {
        //Run the window.
        window.Run();
    }

    public void Dispose()
    {
        renderPipeline?.Dispose();

        cameraBindGroup?.Dispose();

        indexBuffer?.Dispose();
        vertexBuffer?.Dispose();
        modelBindGroup?.Dispose();
        textureView?.Dispose();
        texture?.Dispose();
        sampler?.Dispose();
        shader?.Dispose();

        depthTexture?.Dispose();

        Graphics?.Dispose();
        input?.Dispose();
        input = null;

        window?.Dispose();
        window = null!;
    }

    private void FramebufferResize(Vector2D<int> size)
    {
        Graphics.ResizeSwapChain();
        CreateDepthTexture(size);
        UpdateProjectionMatrix();
    }

    private void OnLoad()
    {
        input = window.CreateInput();
        keyboard = input.Keyboards[0];

        CreateDepthTexture(window.FramebufferSize);

        var shaderCode =
            """
            struct VertexOutputs {
                //The position of the vertex
                @builtin(position) position: vec4<f32>,
                //The texture cooridnate of the vertex
                @location(0) tex_coord: vec2<f32>,
                @location(1) color: vec4<f32>
            
            }

            @group(1) @binding(0) var<uniform> view_matrix: mat4x4<f32>;
            @group(1) @binding(1) var<uniform> projection_matrix: mat4x4<f32>;
            

            @vertex
            fn vs_main(
                @location(0) pos: vec3<f32>,
                @location(1) tex_coord: vec2<f32>,
                @location(2) normal: vec3<f32>,
                @location(3) color: vec4<f32>
            
            ) -> VertexOutputs {
                var output: VertexOutputs;

                var mat = projection_matrix * view_matrix * world_matrix;

                output.position =  mat * vec4<f32>(pos, 1.0);
                output.tex_coord = tex_coord;
                output.color = color;
                return output;
            }

            //The texture we're sampling
            @group(0) @binding(0) var t: texture_2d<f32>;
            //The sampler we're using to sample the texture
            @group(0) @binding(1) var s: sampler;
            @group(0) @binding(2) var<uniform> world_matrix: mat4x4<f32>;

            @fragment
            fn fs_main(input: VertexOutputs) -> @location(0) vec4<f32> {
                var color = textureSample(t, s, input.tex_coord);

                return mix(input.color, vec4<f32>(color.rgb, 1), color.a); 
            }
            """;

        shader = Graphics.Device.CreateShaderModuleWGSL(shaderCode);

        texture = Graphics.Device.LoadTexture("silk.png", TextureFormat.Rgba8Unorm);
        textureView = texture.CreateView();

        sampler = Graphics.Device.CreateSampler(FilterMode.Linear, MipmapFilterMode.Linear);

        modelBindGroup = new ModelBindGroup(Graphics.Device, textureView, sampler)
        {
            World = Matrix4x4.Identity,
        };
        cameraBindGroup = new CameraBindGroup(Graphics.Device);
        UpdateProjectionMatrix();

        //Get a queue
        using var queue = Graphics.Device.GetQueue();

        vertexBuffer = Graphics.Device.CreateBuffer<Vertex>(BufferUsage.Vertex | BufferUsage.CopyDst, (ulong)cube.Verticies.Length);
        queue.WriteBuffer(vertexBuffer, [.. cube.Verticies]);

        indexBuffer = Graphics.Device.CreateBuffer<uint>(BufferUsage.Index | BufferUsage.CopyDst, (ulong)cube.Indices.Length);
        queue.WriteBuffer(indexBuffer, [.. cube.Indices]);


        CreateRenderPipeline();
    }

    private void CreateDepthTexture(Vector2D<int> framebufferSize)
    {
        depthTexture?.Dispose();

        depthTexture = Graphics.Device.CreateTexture(
            framebufferSize,
            TextureFormat.Depth24Plus,
            TextureUsage.RenderAttachment);
    }

    private unsafe void CreateRenderPipeline()
    {
        using var pipelineLayout = Graphics.Device!.CreatePipelineLayout(
            modelBindGroup,
            cameraBindGroup);

        renderPipeline = RenderPipelineDescriptor.Empty
                            .WithLayout(pipelineLayout)
                            .WithVertex<Vertex>(shader!, "vs_main")
                            .WithPrimitive(PrimitiveTopology.TriangleList, CullMode.Back)
                            .WithMultisampleState()
                            .WithFragment(shader!, "fs_main", Graphics.DefaultSurfaceFormat, BlendStates.NonPremultiplied)
                            .WithDepthStencil(TextureFormat.Depth24Plus, CompareFunction.Less)
                            .Create(Graphics.Device);
    }

    private unsafe void UpdateProjectionMatrix()
    {
        cameraBindGroup!.Projection
            = Matrix4x4.CreatePerspectiveFieldOfView(
                MathF.PI / 2,
                window.FramebufferSize.X / window.FramebufferSize.Y,
                0.1f,
                100.0f);

        cameraBindGroup.View = Matrix4x4.CreateLookAt(new(0, 5, 5), new Vector3(0, 0, 0), Vector3.UnitY);
    }

    private void OnUpdate(double delta)
    {
        if (keyboard!.IsKeyPressed(Key.Escape))
        {
            window!.Close();
        }

        var rotation = Matrix4x4.CreateFromYawPitchRoll((float)Math.Sin(window.Time), (float)Math.Cos(window.Time), 0);
        var translation = Matrix4x4.CreateTranslation(0, (float)Math.Sin(window.Time), 0);
        modelBindGroup.World = rotation * translation;

        totalTime += delta;
        timer.Enqueue(delta);
        while (totalTime > 1) totalTime -= timer.Dequeue();
        var fps = timer.Count;

        window.Title = $"WebGPU - FPS: {fps}";

    }

    private double totalTime = 0;
    private Queue<double> timer = new Queue<double>();

    private unsafe void OnRender(double delta)
    {
        if (renderPipeline is null || vertexBuffer is null || indexBuffer is null) return;

        using var surfaceTextureView = Graphics.GetCurrentSurfaceTextureView();
        if (surfaceTextureView is null) return;

        using var depthView = depthTexture?.CreateView();

        var renderPassDesc = RenderPassDescriptor.Empty
                                .WithColorAttachment(surfaceTextureView, Color.Black)
                                .WithDepthStencilAttachment(depthView!);

        using var commandEncoder = Graphics.Device!.CreateCommandEncoder();

        using var renderPassEncoder = commandEncoder.BeginRenderPass(in renderPassDesc);
        renderPassEncoder.SetPipeline(renderPipeline);
        renderPassEncoder.SetBindGroup(0, modelBindGroup!);
        renderPassEncoder.SetBindGroup(1, cameraBindGroup!);
        renderPassEncoder.SetVertexBuffer(0, vertexBuffer, 0, vertexBuffer.Size);
        renderPassEncoder.SetIndexBuffer(indexBuffer, IndexFormat.Uint32, 0, indexBuffer.Size);
        renderPassEncoder.DrawIndexed((uint)cube.Indices.Length, 1, 0, 0, 0);
        renderPassEncoder.End();
        using var commandBuffer = commandEncoder.Finish();
        Graphics.Queue!.Submit(commandBuffer);
        Graphics.Surface.Present();
        window!.SwapBuffers();
    }
}


internal record MeshData<T>(T[] Verticies, uint[] Indices, bool IsLeftHanded);

internal static class Shapes
{
    private const int CubeFaceCount = 6;

    private static readonly Vector3[] FaceNormals = new Vector3[CubeFaceCount]
    {
        new Vector3(0, 0, 1),
        new Vector3(0, 0, -1),
        new Vector3(1, 0, 0),
        new Vector3(-1, 0, 0),
        new Vector3(0, 1, 0),
        new Vector3(0, -1, 0),
    };

    private static readonly Vector2[] TextureCoordinates = new Vector2[4]
    {
        new Vector2(1, 0),
        new Vector2(1, 1),
        new Vector2(0, 1),
        new Vector2(0, 0),
    };

    public static MeshData<Vertex> Cube(Vector3 size, float uScale = 1.0f, float vScale = 1.0f, Vector4? color = null, bool toLeftHanded = false)
    {
        var vertices = new Vertex[CubeFaceCount * 4];
        var indices = new uint[CubeFaceCount * 6];

        var texCoords = new Vector2[4];
        for (var i = 0; i < 4; i++)
        {
            texCoords[i] = TextureCoordinates[i] * new Vector2(uScale, vScale);
        }

        size /= 2.0f;

        int vertexCount = 0;
        int indexCount = 0;
        // Create each face in turn.
        for (uint i = 0; i < CubeFaceCount; i++)
        {
            Vector3 normal = FaceNormals[i];

            // Get two vectors perpendicular both to the face normal and to each other.
            Vector3 basis = (i >= 4) ? Vector3.UnitZ : Vector3.UnitY;

            Vector3 side1 = Vector3.Cross(normal, basis);

            Vector3 side2 = Vector3.Cross(normal, side1);

            // Six indices (two triangles) per face.
            uint vbase = i * 4;
            indices[indexCount++] = (vbase + 0);
            indices[indexCount++] = (vbase + 1);
            indices[indexCount++] = (vbase + 2);

            indices[indexCount++] = (vbase + 0);
            indices[indexCount++] = (vbase + 2);
            indices[indexCount++] = (vbase + 3);

            // Four vertices per face.
            vertices[vertexCount++] = new Vertex((normal - side1 - side2) * size, texCoords[0], normal, color ?? new Vector4(1, 1, 1, 1));
            vertices[vertexCount++] = new Vertex((normal - side1 + side2) * size, texCoords[1], normal, color ?? new Vector4(1, 1, 1, 1));
            vertices[vertexCount++] = new Vertex((normal + side1 + side2) * size, texCoords[2], normal, color ?? new Vector4(1, 1, 1, 1));
            vertices[vertexCount++] = new Vertex((normal + side1 - side2) * size, texCoords[3], normal, color ?? new Vector4(1, 1, 1, 1));
        }

        // Create the primitive object.
        return new MeshData<Vertex>(vertices, indices, toLeftHanded);
    }
}
