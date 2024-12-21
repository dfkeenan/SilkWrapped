namespace SilkWrapped.WebGPU;
public partial struct QuerySetDescriptor
{
    public unsafe string? Label;
    public QueryType Type;
    public uint Count;
    public unsafe QuerySetDescriptor(string? label = null, QueryType? type = null, uint? count = null)
    {
        this = default(QuerySetDescriptor);
        if (label != null)
        {
            Label = label;
        }

        if (type.HasValue)
        {
            Type = type.Value;
        }

        if (count.HasValue)
        {
            Count = count.Value;
        }
    }
}