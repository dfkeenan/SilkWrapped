using Silk.NET.Core;
using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct RequestAdapterOptions
{
    public unsafe SurfaceHandle CompatibleSurface;
    public PowerPreference PowerPreference;
    public BackendType BackendType;
    public Bool32 ForceFallbackAdapter;
    public unsafe RequestAdapterOptions(SurfaceHandle? compatibleSurface = null, PowerPreference? powerPreference = null, BackendType? backendType = null, Bool32? forceFallbackAdapter = null)
    {
        this = default(RequestAdapterOptions);
        if (compatibleSurface.HasValue)
        {
            CompatibleSurface = compatibleSurface.Value;
        }

        if (powerPreference.HasValue)
        {
            PowerPreference = powerPreference.Value;
        }

        if (backendType.HasValue)
        {
            BackendType = backendType.Value;
        }

        if (forceFallbackAdapter.HasValue)
        {
            ForceFallbackAdapter = forceFallbackAdapter.Value;
        }
    }
}