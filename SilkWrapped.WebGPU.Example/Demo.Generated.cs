//using System.Numerics;
//using System.Runtime.CompilerServices;

//namespace SilkWrapped.WebGPU.Example;

//internal readonly partial record struct Vertex : IVertexStruct
//{
//    public static VertexBufferLayout GetLayout()
//    {
//        int offset = 0;
//        var attributes = new VertexAttribute[2];

//        attributes[0] = new()
//        {
//            Format = VertexFormat.Float32x2,
//            Offset = (ulong)offset,
//            ShaderLocation = 0
//        };
//        offset += Unsafe.SizeOf<Vector2>();

//        attributes[1] = new VertexAttribute
//        {
//            Format = VertexFormat.Float32x2,
//            Offset = (ulong)offset,
//            ShaderLocation = 1
//        };
//        offset += Unsafe.SizeOf<Vector2>();

//        var vertexBufferLayout = new VertexBufferLayout
//        {
//            Attributes = attributes,
//            StepMode = VertexStepMode.Vertex,
//            ArrayStride = (ulong)Unsafe.SizeOf<Vertex>()
//        };

//        return vertexBufferLayout;
//    }
//}


//internal partial class ProjectionMatrixBindGroup : IBindGroup<ProjectionMatrixBindGroup>
//{
//    private readonly Device device = device;
//    private BindGroupLayout layout;
//    private BindGroup group;
//    private Changes changes = Changes.None;

//    private Buffer<Matrix4x4> projectionBuffer;
//    private Matrix4x4 projection;
//    public partial Matrix4x4 Projection
//    {
//        get => this.projection;
//        set
//        {
//            this.projection = value;
//            changes |= Changes.Projection;
//        }
//    }

//    public BindGroupLayout Layout
//    {
//        get
//        {
//            //TODO - How to share layout between instances
//            if (layout is not null) return layout;

//            layout = CreateLayout(device);
//            return layout;
//        }
//    }

//    public static BindGroupLayout CreateLayout(Device device)
//    {
//        var layoutDescriptor = new BindGroupLayoutDescriptor
//        {
//            Entries =
//                [
//                    new BindGroupLayoutEntry
//                    {
//                        Binding = 0,
//                        Buffer = new BufferBindingLayout
//                        {
//                            Type = BufferBindingType.Uniform,
//                            MinBindingSize = (ulong)Unsafe.SizeOf<Matrix4x4>(), HasDynamicOffset = true,
//                        },
//                        Visibility = ShaderStage.Vertex,
//                    }
//                ]
//        };

//        return device.CreateBindGroupLayout(in layoutDescriptor);
//    }

//    private void CreateBindGroup()
//    {
//        if (group is not null) return;

//        projectionBuffer ??= device.CreateBuffer<Matrix4x4>(BufferUsage.Uniform | BufferUsage.CopyDst);

//        var descriptor = new BindGroupDescriptor
//        {
//            Layout = Layout,
//            Entries =
//            [
//                new BindGroupEntry
//                {
//                    Binding = 0,
//                    Buffer = projectionBuffer,
//                    Size = (ulong)Unsafe.SizeOf<Matrix4x4>()
//                }
//            ],
//        };

//        group = device.CreateBindGroup(in descriptor);
//    }

//    public void ApplyChanges()
//    {
//        CreateBindGroup();

//        using var queue = device.GetQueue();

//        if (changes.HasFlag(Changes.Projection))
//        {
//            queue.WriteBuffer(projectionBuffer, projection);
//        }

//        changes = Changes.None;
//    }

//    public void Dispose()
//    {
//        group?.Dispose();
//        group = null;
//        layout?.Dispose();
//        layout = null;
//        projectionBuffer?.Dispose();
//        projectionBuffer = null;
//    }

//    public bool Equals(ProjectionMatrixBindGroup other)
//    {
//        if (other is null) return false;

//        CreateBindGroup();
//        other.CreateBindGroup();

//        return group.Equals(other.group);
//    }

//    public override bool Equals(object obj)
//    {
//        if (obj is not ProjectionMatrixBindGroup other) return false;
//        return Equals(other);
//    }

//    public override int GetHashCode()
//    {
//        CreateBindGroup();
//        return group.GetHashCode();
//    }

//    public static implicit operator BindGroupLayoutHandle(ProjectionMatrixBindGroup obj)
//        => obj.Layout;

//    public static explicit operator BindGroupHandle(ProjectionMatrixBindGroup obj)
//    {
//        obj.CreateBindGroup();
//        return obj.group;
//    }

//    [Flags]
//    private enum Changes
//    {
//        None = 0,
//        Projection = 1 << 0,
//    }
//}

//internal partial class TextureBindGroup : IBindGroup<TextureBindGroup>
//{
//    private readonly Device device = device;
//    private BindGroupLayout layout;
//    private BindGroup group;

//    private TextureView textureView = textureView ?? throw new ArgumentNullException(nameof(textureView));
//    private Sampler sampler = sampler ?? throw new ArgumentNullException(nameof(sampler));

//    public BindGroupLayout Layout
//    {
//        get
//        {
//            //TODO - How to share layout between instances
//            if (layout is not null) return layout;

//            layout = CreateLayout(device);
//            return layout;
//        }
//    }

//    public static BindGroupLayout CreateLayout(Device device)
//    {
//        var layoutDescriptor = new BindGroupLayoutDescriptor
//        {
//            Entries =
//                [
//                    new BindGroupLayoutEntry
//                    {
//                        Binding = 0,
//                        Texture = new TextureBindingLayout
//                        {
//                            Multisampled = false,
//                            SampleType = TextureSampleType.Float,
//                            ViewDimension = TextureViewDimension.Dimension2D
//                        },
//                        Visibility = ShaderStage.Fragment
//                    },
//                    new BindGroupLayoutEntry
//                    {
//                        Binding = 1,
//                        Sampler = new SamplerBindingLayout
//                        {
//                            Type = SamplerBindingType.Filtering
//                        },
//                        Visibility = ShaderStage.Fragment
//                    }
//                ]
//        };

//        return device.CreateBindGroupLayout(in layoutDescriptor);
//    }

//    private void CreateBindGroup()
//    {
//        if (group is not null) return;

//        var descriptor = new BindGroupDescriptor
//        {
//            Layout = Layout,
//            Entries =
//            [
//                new BindGroupEntry
//                {
//                    Binding = 0,
//                    TextureView = textureView
//                },
//                new BindGroupEntry
//                {
//                    Binding = 1,
//                    Sampler = sampler
//                }
//            ],
//        };

//        group = device.CreateBindGroup(in descriptor);
//    }

//    public void ApplyChanges()
//    {
//        CreateBindGroup();
//    }

//    public void Dispose()
//    {
//        group?.Dispose();
//        group = null;
//        layout?.Dispose();
//        layout = null;
//    }

//    public bool Equals(TextureBindGroup other)
//    {
//        if (other is null) return false;

//        CreateBindGroup();
//        other.CreateBindGroup();

//        return group.Equals(other.group);
//    }

//    public override bool Equals(object obj)
//    {
//        if (obj is not TextureBindGroup other) return false;
//        return Equals(other);
//    }

//    public override int GetHashCode()
//    {
//        CreateBindGroup();
//        return group.GetHashCode();
//    }

//    public static implicit operator BindGroupLayoutHandle(TextureBindGroup obj)
//        => obj.Layout;

//    public static explicit operator BindGroupHandle(TextureBindGroup obj)
//    {
//        obj.CreateBindGroup();
//        return obj.group;
//    }
//}

////internal partial class TextureBindGroup
////{

////}