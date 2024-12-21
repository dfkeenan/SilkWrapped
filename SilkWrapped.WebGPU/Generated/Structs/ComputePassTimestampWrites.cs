using Silk.NET.Core.Attributes;

namespace SilkWrapped.WebGPU;
public partial struct ComputePassTimestampWrites
{
    public unsafe QuerySetHandle QuerySet;
    public uint BeginningOfPassWriteIndex;
    public uint EndOfPassWriteIndex;
    public unsafe ComputePassTimestampWrites(QuerySetHandle? querySet = null, uint? beginningOfPassWriteIndex = null, uint? endOfPassWriteIndex = null)
    {
        this = default(ComputePassTimestampWrites);
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