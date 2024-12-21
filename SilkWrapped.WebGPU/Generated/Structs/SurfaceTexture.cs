using Silk.NET.Core;
using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct SurfaceTexture
{
    public unsafe TextureHandle Texture;
    public Bool32 Suboptimal;
    public SurfaceGetCurrentTextureStatus Status;
    public unsafe SurfaceTexture(TextureHandle? texture = null, Bool32? suboptimal = null, SurfaceGetCurrentTextureStatus? status = null)
    {
        this = default(SurfaceTexture);
        if (texture.HasValue)
        {
            Texture = texture.Value;
        }

        if (suboptimal.HasValue)
        {
            Suboptimal = suboptimal.Value;
        }

        if (status.HasValue)
        {
            Status = status.Value;
        }
    }
}