using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct ImageCopyBuffer
{
    public TextureDataLayout Layout;
    public unsafe BufferHandle Buffer;
    public unsafe ImageCopyBuffer(TextureDataLayout? layout = null, BufferHandle? buffer = null)
    {
        this = default(ImageCopyBuffer);
        if (layout.HasValue)
        {
            Layout = layout.Value;
        }

        if (buffer.HasValue)
        {
            Buffer = buffer.Value;
        }
    }
}