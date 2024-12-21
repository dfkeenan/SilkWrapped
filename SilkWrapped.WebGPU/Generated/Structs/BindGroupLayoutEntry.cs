namespace SilkWrapped.WebGPU;
public partial struct BindGroupLayoutEntry
{
    public uint Binding;
    public ShaderStage Visibility;
    public BufferBindingLayout Buffer;
    public SamplerBindingLayout Sampler;
    public TextureBindingLayout Texture;
    public StorageTextureBindingLayout StorageTexture;
    public unsafe BindGroupLayoutEntry(uint? binding = null, ShaderStage? visibility = null, BufferBindingLayout? buffer = null, SamplerBindingLayout? sampler = null, TextureBindingLayout? texture = null, StorageTextureBindingLayout? storageTexture = null)
    {
        this = default(BindGroupLayoutEntry);
        if (binding.HasValue)
        {
            Binding = binding.Value;
        }

        if (visibility.HasValue)
        {
            Visibility = visibility.Value;
        }

        if (buffer.HasValue)
        {
            Buffer = buffer.Value;
        }

        if (sampler.HasValue)
        {
            Sampler = sampler.Value;
        }

        if (texture.HasValue)
        {
            Texture = texture.Value;
        }

        if (storageTexture.HasValue)
        {
            StorageTexture = storageTexture.Value;
        }
    }
}