namespace SilkWrapped.WebGPU;
public partial struct TextureDataLayout
{
    public ulong Offset;
    public uint BytesPerRow;
    public uint RowsPerImage;
    public unsafe TextureDataLayout(ulong? offset = null, uint? bytesPerRow = null, uint? rowsPerImage = null)
    {
        this = default(TextureDataLayout);
        if (offset.HasValue)
        {
            Offset = offset.Value;
        }

        if (bytesPerRow.HasValue)
        {
            BytesPerRow = bytesPerRow.Value;
        }

        if (rowsPerImage.HasValue)
        {
            RowsPerImage = rowsPerImage.Value;
        }
    }
}