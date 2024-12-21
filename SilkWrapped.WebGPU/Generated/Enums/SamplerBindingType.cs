namespace SilkWrapped.WebGPU;
public enum SamplerBindingType
{
    Undefined = 0,
    Filtering = 1,
    NonFiltering = 2,
    Comparison = 3,
    Force32 = int.MaxValue
}