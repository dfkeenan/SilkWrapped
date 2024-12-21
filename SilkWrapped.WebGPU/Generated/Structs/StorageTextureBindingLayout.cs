using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct StorageTextureBindingLayout
{
    public StorageTextureAccess Access;
    public TextureFormat Format;
    public TextureViewDimension ViewDimension;
    public unsafe StorageTextureBindingLayout(StorageTextureAccess? access = null, TextureFormat? format = null, TextureViewDimension? viewDimension = null)
    {
        this = default(StorageTextureBindingLayout);
        if (access.HasValue)
        {
            Access = access.Value;
        }

        if (format.HasValue)
        {
            Format = format.Value;
        }

        if (viewDimension.HasValue)
        {
            ViewDimension = viewDimension.Value;
        }
    }
}