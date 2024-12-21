namespace SilkWrapped.WebGPU;
public unsafe partial class Texture
{
    public TextureView CreateView()
    {
        var result = WebGPU.TextureCreateView(Handle, null);
        if (result == null)
            return null;
        return new TextureView(WebGPU, result);
    }
}
