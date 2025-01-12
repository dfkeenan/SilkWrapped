namespace SilkWrapped.WebGPU;
public interface IShader : IDisposable
{
    string EntryPoint { get; }
    ShaderModule Module { get; }
}

public record Shader(string EntryPoint, ShaderModule Module) : IShader
{
    public void Dispose() => Module.Dispose();
}
