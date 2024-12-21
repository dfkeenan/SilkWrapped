namespace SilkWrapped.WebGPU;
public enum TextureAspect
{
    All = 0,
    StencilOnly = 1,
    DepthOnly = 2,
    Force32 = int.MaxValue
}