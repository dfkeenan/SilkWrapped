namespace SilkWrapped.WebGPU;

public interface IBindGroup<TBindGroup> : IEquatable<TBindGroup>, IDisposable
    where TBindGroup : class, IBindGroup<TBindGroup>
{
    BindGroupLayout Layout { get; }
    void ApplyChanges();

    static abstract BindGroupLayout CreateLayout(Device device);

    static abstract implicit operator BindGroupLayoutHandle(TBindGroup obj);
    static abstract explicit operator BindGroupHandle(TBindGroup obj);
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class BindGroupAttribute : Attribute;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
public abstract class BindingAttribute(ShaderStage visibility) : Attribute
{
    public ShaderStage Visibility { get; } = visibility;
}

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
public abstract class BufferBindingAttribute(BufferBindingType type, ShaderStage visibility)
    : BindingAttribute(visibility)
{
    public BufferBindingType Type { get; } = type;
    public bool HasDynamicOffset { get; set; } = false;
}

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
public class UniformBindingAttribute(ShaderStage visibility)
    : BufferBindingAttribute(BufferBindingType.Uniform, visibility)
{

}

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
public sealed class SamplerBindingAttribute(SamplerBindingType type, ShaderStage visibility)
    : BindingAttribute(visibility)
{
    public SamplerBindingType Type { get; } = type;
}

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
public sealed class TextureBindingAttribute(TextureSampleType sampleType, TextureViewDimension viewDimension, ShaderStage visibility)
    : BindingAttribute(visibility)
{
    public TextureSampleType SampleType { get; } = sampleType;
    public TextureViewDimension ViewDimension { get; } = viewDimension;
    public bool Multisampled { get; set; } = false;
}

//TODO: What doe these do
//[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
//public sealed class StorageBindingAttribute(StorageTextureAccess access, TextureFormat format, TextureViewDimension viewDimension, ShaderStage visibility)
//    : BindingAttribute(visibility)
//{
//    public StorageTextureAccess Access { get; } = access;
//    public TextureFormat Format { get; } = format;
//    public TextureViewDimension ViewDimension { get; } = viewDimension;
//}
