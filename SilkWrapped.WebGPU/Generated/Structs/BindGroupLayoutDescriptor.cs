using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct BindGroupLayoutDescriptor
{
    public unsafe string? Label;
    public unsafe BindGroupLayoutEntry[]? Entries;
    public unsafe BindGroupLayoutDescriptor(string? label = null, BindGroupLayoutEntry[]? entries = null)
    {
        this = default(BindGroupLayoutDescriptor);
        if (label != null)
        {
            Label = label;
        }

        if (entries != null)
        {
            Entries = entries;
        }
    }
}