namespace SilkWrapped.WebGPU;
public partial struct BindGroupDescriptor
{
    public unsafe string? Label;
    public unsafe BindGroupLayoutHandle Layout;
    public unsafe BindGroupEntry[]? Entries;
    public unsafe BindGroupDescriptor(string? label = null, BindGroupLayoutHandle? layout = null, BindGroupEntry[]? entries = null)
    {
        this = default(BindGroupDescriptor);
        if (label != null)
        {
            Label = label;
        }

        if (layout.HasValue)
        {
            Layout = layout.Value;
        }

        if (entries != null)
        {
            Entries = entries;
        }
    }
}