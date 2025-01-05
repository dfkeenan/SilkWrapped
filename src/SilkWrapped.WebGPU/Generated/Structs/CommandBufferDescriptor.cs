namespace SilkWrapped.WebGPU;
public partial struct CommandBufferDescriptor
{
    public unsafe string? Label;
    public unsafe CommandBufferDescriptor(string? label = null)
    {
        this = default(CommandBufferDescriptor);
        if (label != null)
        {
            Label = label;
        }
    }
}