using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct AdapterProperties
{
    public uint VendorID;
    public unsafe string? VendorName;
    public unsafe string? Architecture;
    public uint DeviceID;
    public unsafe string? Name;
    public unsafe string? DriverDescription;
    public AdapterType AdapterType;
    public BackendType BackendType;
    public unsafe AdapterProperties(uint? vendorID = null, string? vendorName = null, string? architecture = null, uint? deviceID = null, string? name = null, string? driverDescription = null, AdapterType? adapterType = null, BackendType? backendType = null)
    {
        this = default(AdapterProperties);
        if (vendorID.HasValue)
        {
            VendorID = vendorID.Value;
        }

        if (vendorName != null)
        {
            VendorName = vendorName;
        }

        if (architecture != null)
        {
            Architecture = architecture;
        }

        if (deviceID.HasValue)
        {
            DeviceID = deviceID.Value;
        }

        if (name != null)
        {
            Name = name;
        }

        if (driverDescription != null)
        {
            DriverDescription = driverDescription;
        }

        if (adapterType.HasValue)
        {
            AdapterType = adapterType.Value;
        }

        if (backendType.HasValue)
        {
            BackendType = backendType.Value;
        }
    }
}