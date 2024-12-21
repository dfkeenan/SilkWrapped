using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct ComputePipelineDescriptor
{
    public unsafe string? Label;
    public unsafe PipelineLayoutHandle Layout;
    public ProgrammableStageDescriptor Compute;
    public unsafe ComputePipelineDescriptor(string? label = null, PipelineLayoutHandle? layout = null, ProgrammableStageDescriptor? compute = null)
    {
        this = default(ComputePipelineDescriptor);
        if (label != null)
        {
            Label = label;
        }

        if (layout.HasValue)
        {
            Layout = layout.Value;
        }

        if (compute.HasValue)
        {
            Compute = compute.Value;
        }
    }
}