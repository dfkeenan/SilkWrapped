namespace SilkWrapped.WebGPU;
public partial struct AdapterInfo
{
    public unsafe string? Vendor;
    public unsafe string? Architecture;
    public unsafe string? Device;
    public unsafe string? Description;
    public unsafe AdapterInfo(string? vendor = null, string? architecture = null, string? device = null, string? description = null)
    {
        this = default(AdapterInfo);
        if (vendor != null)
        {
            Vendor = vendor;
        }

        if (architecture != null)
        {
            Architecture = architecture;
        }

        if (device != null)
        {
            Device = device;
        }

        if (description != null)
        {
            Description = description;
        }
    }
}