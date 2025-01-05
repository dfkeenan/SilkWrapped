namespace SilkWrapped.WebGPU;
public partial struct SurfaceConfiguration
{
    public unsafe DeviceHandle Device;
    public TextureFormat Format;
    public TextureUsage Usage;
    public unsafe TextureFormat[]? ViewFormats;
    public CompositeAlphaMode AlphaMode;
    public uint Width;
    public uint Height;
    public PresentMode PresentMode;
    public unsafe SurfaceConfiguration(DeviceHandle? device = null, TextureFormat? format = null, TextureUsage? usage = null, TextureFormat[]? viewFormats = null, CompositeAlphaMode? alphaMode = null, uint? width = null, uint? height = null, PresentMode? presentMode = null)
    {
        this = default(SurfaceConfiguration);
        if (device.HasValue)
        {
            Device = device.Value;
        }

        if (format.HasValue)
        {
            Format = format.Value;
        }

        if (usage.HasValue)
        {
            Usage = usage.Value;
        }

        if (viewFormats != null)
        {
            ViewFormats = viewFormats;
        }

        if (alphaMode.HasValue)
        {
            AlphaMode = alphaMode.Value;
        }

        if (width.HasValue)
        {
            Width = width.Value;
        }

        if (height.HasValue)
        {
            Height = height.Value;
        }

        if (presentMode.HasValue)
        {
            PresentMode = presentMode.Value;
        }
    }
}