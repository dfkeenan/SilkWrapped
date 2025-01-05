namespace SilkWrapped.WebGPU;
public partial struct ImageCopyTexture
{
    public unsafe TextureHandle Texture;
    public uint MipLevel;
    public Origin3D Origin;
    public TextureAspect Aspect;
    public unsafe ImageCopyTexture(TextureHandle? texture = null, uint? mipLevel = null, Origin3D? origin = null, TextureAspect? aspect = null)
    {
        this = default(ImageCopyTexture);
        if (texture.HasValue)
        {
            Texture = texture.Value;
        }

        if (mipLevel.HasValue)
        {
            MipLevel = mipLevel.Value;
        }

        if (origin.HasValue)
        {
            Origin = origin.Value;
        }

        if (aspect.HasValue)
        {
            Aspect = aspect.Value;
        }
    }
}