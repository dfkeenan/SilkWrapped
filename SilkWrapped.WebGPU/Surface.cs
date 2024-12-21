namespace SilkWrapped.WebGPU;
public unsafe partial class Surface
{
    public (SurfaceGetCurrentTextureStatus Status, Texture Texture) GetCurrentTexture()
    {
        SurfaceTexture surfaceTexture = default;
        GetCurrentTexture(ref surfaceTexture);

        return (surfaceTexture.Status, new Texture(WebGPU, surfaceTexture.Texture));
    }
}
