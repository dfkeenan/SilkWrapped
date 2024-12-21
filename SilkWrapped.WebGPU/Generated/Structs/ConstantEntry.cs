using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct ConstantEntry
{
    public unsafe string? Key;
    public double Value;
    public unsafe ConstantEntry(string? key = null, double? value = null)
    {
        this = default(ConstantEntry);
        if (key != null)
        {
            Key = key;
        }

        if (value.HasValue)
        {
            Value = value.Value;
        }
    }
}