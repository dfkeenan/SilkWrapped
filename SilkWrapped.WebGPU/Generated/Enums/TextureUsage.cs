namespace SilkWrapped.WebGPU;
[Flags]
public enum TextureUsage
{
    None = 0,
    CopySrc = 1,
    CopyDst = 2,
    TextureBinding = 4,
    StorageBinding = 8,
    RenderAttachment = 0x10,
    Force32 = int.MaxValue
}