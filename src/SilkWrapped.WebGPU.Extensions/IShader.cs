namespace SilkWrapped.WebGPU;
public interface IShader
{
    string EntryPoint { get; }
    ShaderModule Module { get; }
}

public record Shader(string EntryPoint, ShaderModule Module) : IShader;
