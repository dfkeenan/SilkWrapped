using System.Numerics;
using System.Runtime.InteropServices;

namespace SilkWrapped.WebGPU.Example;
[VertexStruct]
[StructLayout(LayoutKind.Sequential)]
internal readonly partial record struct DemoVertex(Vector3 Position, Vector2 TexCoord, Vector3 Normal, Vector4 Color);

[BindGroup]
internal partial class DemoCameraBindGroup(Device device)
{
    [UniformBinding(ShaderStage.Vertex)]
    public partial Matrix4x4 View { get; set; }

    [UniformBinding(ShaderStage.Vertex)]
    public partial Matrix4x4 Projection { get; set; }
}

[BindGroup]
internal partial class DemoModelBindGroup(
     Device device,
     [TextureBinding(TextureSampleType.Float, TextureViewDimension.Dimension2D, ShaderStage.Fragment)] TextureView textureView,
     [SamplerBinding(SamplerBindingType.Filtering, ShaderStage.Fragment)] Sampler sampler)
{
    [UniformBinding(ShaderStage.Vertex)]
    public partial Matrix4x4 World { get; set; }
}

internal class DemoRenderPipeline(Device device, TextureFormat surfaceFormat, TextureFormat depthFormat) : CustomRenderPipeline
{
    private readonly DemoVertexShader vertexShader = new(device);
    private readonly DemoFragmentShader fragmentShader = new(device);

    protected override RenderPipeline CreatePipeline()
    {
        //TODO: Share/Reuse BindGroupLayouts and PipelineLayouts
        using var modelBindGroupLayout = DemoModelBindGroup.CreateLayout(device);

        using var cameraBindGroupLayout = DemoCameraBindGroup.CreateLayout(device);
        using var pipelineLayout = device.CreatePipelineLayout(
            modelBindGroupLayout,
            cameraBindGroupLayout);

        return RenderPipelineDescriptor.Empty
                            .WithLayout(pipelineLayout)
                            .WithVertex<DemoVertex>(vertexShader)
                            .WithPrimitive(PrimitiveTopology.TriangleList, CullMode.Back)
                            .WithMultisampleState()
                            .WithFragment(fragmentShader, surfaceFormat, BlendStates.NonPremultiplied)
                            .WithDepthStencil(depthFormat, CompareFunction.Less)
                            .Create(device);
    }

    protected override void Disposing()
    {
        vertexShader.Dispose();
        fragmentShader.Dispose();
    }
}

internal record DemoVertexShader(Device device)
    : Shader("vs_main", device.CreateShaderModuleWGSL(
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
        @group(0) @binding(2) var<uniform> world_matrix: mat4x4<f32>;
        
        
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
        """u8 + "\0"u8));

internal record DemoFragmentShader(Device device)
    : Shader("fs_main", device.CreateShaderModuleWGSL(
        """
        struct VertexOutputs {
            //The position of the vertex
            @builtin(position) position: vec4<f32>,
            //The texture cooridnate of the vertex
            @location(0) tex_coord: vec2<f32>,
            @location(1) color: vec4<f32>
        
        }
        
        //The texture we're sampling
        @group(0) @binding(0) var t: texture_2d<f32>;
        //The sampler we're using to sample the texture
        @group(0) @binding(1) var s: sampler;
        
        @fragment
        fn fs_main(input: VertexOutputs) -> @location(0) vec4<f32> {
            var color = textureSample(t, s, input.tex_coord);
        
            return mix(input.color, vec4<f32>(color.rgb, 1), color.a); 
        }
        """u8 + "\0"u8));