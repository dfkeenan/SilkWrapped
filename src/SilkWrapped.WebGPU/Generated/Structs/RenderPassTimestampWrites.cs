namespace SilkWrapped.WebGPU;
public partial struct RenderPassTimestampWrites
{
    public unsafe QuerySetHandle QuerySet;
    public uint BeginningOfPassWriteIndex;
    public uint EndOfPassWriteIndex;
    public unsafe RenderPassTimestampWrites(QuerySetHandle? querySet = null, uint? beginningOfPassWriteIndex = null, uint? endOfPassWriteIndex = null)
    {
        this = default(RenderPassTimestampWrites);
        if (querySet.HasValue)
        {
            QuerySet = querySet.Value;
        }

        if (beginningOfPassWriteIndex.HasValue)
        {
            BeginningOfPassWriteIndex = beginningOfPassWriteIndex.Value;
        }

        if (endOfPassWriteIndex.HasValue)
        {
            EndOfPassWriteIndex = endOfPassWriteIndex.Value;
        }
    }
}