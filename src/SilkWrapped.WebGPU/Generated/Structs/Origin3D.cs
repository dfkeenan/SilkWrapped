namespace SilkWrapped.WebGPU;
public partial struct Origin3D
{
    public uint X;
    public uint Y;
    public uint Z;
    public Origin3D(uint? x = null, uint? y = null, uint? z = null)
    {
        this = default(Origin3D);
        if (x.HasValue)
        {
            X = x.Value;
        }

        if (y.HasValue)
        {
            Y = y.Value;
        }

        if (z.HasValue)
        {
            Z = z.Value;
        }
    }
}