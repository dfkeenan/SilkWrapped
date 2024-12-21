using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct Color
{
    public double R;
    public double G;
    public double B;
    public double A;
    public Color(double? r = null, double? g = null, double? b = null, double? a = null)
    {
        this = default(Color);
        if (r.HasValue)
        {
            R = r.Value;
        }

        if (g.HasValue)
        {
            G = g.Value;
        }

        if (b.HasValue)
        {
            B = b.Value;
        }

        if (a.HasValue)
        {
            A = a.Value;
        }
    }
}