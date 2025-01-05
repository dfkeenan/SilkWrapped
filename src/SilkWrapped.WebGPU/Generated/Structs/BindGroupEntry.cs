namespace SilkWrapped.WebGPU;
public partial struct BindGroupEntry
{
    public uint Binding;
    public unsafe BufferHandle Buffer;
    public ulong Offset;
    public ulong Size;
    public unsafe SamplerHandle Sampler;
    public unsafe TextureViewHandle TextureView;
    public unsafe BindGroupEntry(uint? binding = null, BufferHandle? buffer = null, ulong? offset = null, ulong? size = null, SamplerHandle? sampler = null, TextureViewHandle? textureView = null)
    {
        this = default(BindGroupEntry);
        if (binding.HasValue)
        {
            Binding = binding.Value;
        }

        if (buffer.HasValue)
        {
            Buffer = buffer.Value;
        }

        if (offset.HasValue)
        {
            Offset = offset.Value;
        }

        if (size.HasValue)
        {
            Size = size.Value;
        }

        if (sampler.HasValue)
        {
            Sampler = sampler.Value;
        }

        if (textureView.HasValue)
        {
            TextureView = textureView.Value;
        }
    }
}