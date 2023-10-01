namespace SilkWrapped.WebGPU;
public unsafe partial class SurfaceWrapper
{
    public (SurfaceGetCurrentTextureStatus Status, TextureWrapper Texture) GetCurrentTexture()
    {
        SurfaceTexture surfaceTexture = default;
        GetCurrentTexture(ref surfaceTexture);

        return (surfaceTexture.Status, new TextureWrapper(Api, surfaceTexture.Texture));
    }
}
