namespace SilkWrapped.WebGPU;
public readonly struct PfnCompilationInfoCallback : IDisposable
{
    private readonly Silk.NET.WebGPU.PfnCompilationInfoCallback callback;
    public unsafe PfnCompilationInfoCallback(CompilationInfoCallback proc)
    {
        callback = new((status, compilationInfo, data) =>
        {
            using var m = new MarshalHelper();
            CompilationInfo __compilationInfo = default;
            if (compilationInfo != null)
            {
                if (compilationInfo->Messages != null)
                {
                    __compilationInfo.Messages = new CompilationMessage[compilationInfo->MessageCount];
                    for (int i = 0; i < __compilationInfo.Messages!.Length; i++)
                    {
                        __compilationInfo.Messages[i].Message = SilkMarshal.PtrToString((nint)compilationInfo->Messages![i].Message, NativeStringEncoding.UTF8);
                        __compilationInfo.Messages[i].Type = (CompilationMessageType)compilationInfo->Messages![i].Type;
                        __compilationInfo.Messages[i].LineNum = compilationInfo->Messages![i].LineNum;
                        __compilationInfo.Messages[i].LinePos = compilationInfo->Messages![i].LinePos;
                        __compilationInfo.Messages[i].Offset = compilationInfo->Messages![i].Offset;
                        __compilationInfo.Messages[i].Length = compilationInfo->Messages![i].Length;
                        __compilationInfo.Messages[i].Utf16LinePos = compilationInfo->Messages![i].Utf16LinePos;
                        __compilationInfo.Messages[i].Utf16Offset = compilationInfo->Messages![i].Utf16Offset;
                        __compilationInfo.Messages[i].Utf16Length = compilationInfo->Messages![i].Utf16Length;
                    }
                }
            }

            proc((CompilationInfoRequestStatus)status, __compilationInfo, data);
        });
    }

    public static PfnCompilationInfoCallback From(CompilationInfoCallback proc)
    {
        return new PfnCompilationInfoCallback(proc);
    }

    public unsafe void Dispose() => callback.Dispose();
    public static implicit operator Silk.NET.WebGPU.PfnCompilationInfoCallback(PfnCompilationInfoCallback callback) => callback.callback;
}