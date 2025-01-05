using Silk.NET.Windowing;

namespace SilkWrapped.WebGPU;
public static class WebGPUWindowOptions
{
    public static WindowOptions Default => WindowOptions.Default with
    {
        API = GraphicsAPI.None,
        ShouldSwapAutomatically = false,
        IsContextControlDisabled = true,
    };
}
