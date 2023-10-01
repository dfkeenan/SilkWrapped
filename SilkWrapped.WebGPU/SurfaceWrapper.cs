using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.WebGPU;

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
