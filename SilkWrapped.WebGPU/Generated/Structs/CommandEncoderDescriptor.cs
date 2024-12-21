namespace SilkWrapped.WebGPU;
public partial struct CommandEncoderDescriptor
{
    public unsafe string? Label;
    public unsafe CommandEncoderDescriptor(string? label = null)
    {
        this = default(CommandEncoderDescriptor);
        if (label != null)
        {
            Label = label;
        }
    }
}