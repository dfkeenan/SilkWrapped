namespace SilkWrapped.WebGPU;
public partial struct PipelineLayoutDescriptor
{
    public unsafe string? Label;
    public unsafe BindGroupLayoutHandle[]? BindGroupLayouts;
    public unsafe PipelineLayoutDescriptor(string? label = null, BindGroupLayoutHandle[]? bindGroupLayouts = null)
    {
        this = default(PipelineLayoutDescriptor);
        if (label != null)
        {
            Label = label;
        }

        if (bindGroupLayouts != null)
        {
            BindGroupLayouts = bindGroupLayouts;
        }
    }
}