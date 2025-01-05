namespace SilkWrapped.WebGPU;
public enum StorageTextureAccess
{
    Undefined = 0,
    WriteOnly = 1,
    ReadOnly = 2,
    ReadWrite = 3,
    Force32 = int.MaxValue
}