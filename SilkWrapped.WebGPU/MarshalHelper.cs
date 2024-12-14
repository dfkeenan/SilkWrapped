using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SilkWrapped.WebGPU;
internal struct MarshalHelper : IDisposable
{
    public const int MaxStack = 32;
    private const int DefaultCapacity = 4;

    private Action[] disposables = Array.Empty<Action>();
    private int count = 0;

    public MarshalHelper() { }

    public unsafe Span<T> Allocate<T>(int length)
        where T : unmanaged
    {
        var array = ArrayPool<T>.Shared.Rent(length);
        var memory = new Memory<T>(array);
        var handle = memory.Pin();

        var dispose = () =>
        {
            handle.Dispose();
            ArrayPool<T>.Shared.Return(array);
        };

        AddDisposable(dispose);

        return memory.Span;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddDisposable(Action dispose)
    {
        if (count == disposables.Length)
        {
            var newDisposables = ArrayPool<Action>.Shared.Rent(GetNewCapacity(count + 1));
            disposables.CopyTo(newDisposables, 0);
            disposables = newDisposables;
        }

        disposables[count++] = dispose;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetNewCapacity(int capacity)
    {
        Debug.Assert(disposables.Length < capacity);

        int newCapacity = disposables.Length == 0 ? DefaultCapacity : 2 * disposables.Length;

        if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;

        if (newCapacity < capacity) newCapacity = capacity;

        return newCapacity;
    }

    public unsafe T* AllocatePtr<T>(int length)
        where T : unmanaged
    {
        var span = Allocate<T>(length);
        return AsPointer(span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T* AsPointer<T>(Span<T> span)
        where T : unmanaged
    {
        return (T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
    }

    public unsafe Span<byte> Utf8(string? text)
    {
        if(string.IsNullOrEmpty(text)) return Span<byte>.Empty;

        byte[]? arr = null;

        try
        {
            Span<byte> bytes = text.Length <= 256 ? stackalloc byte[text.Length] : arr = ArrayPool<byte>.Shared.Rent(text.Length);

            var length = Encoding.UTF8.GetBytes(text, bytes);
            var utf8Bytes = Allocate<byte>(length + 1);
            bytes.CopyTo(utf8Bytes);
            utf8Bytes[length] = 0;

            return utf8Bytes;
        }
        finally
        {
            if (arr != null)
            {
                ArrayPool<byte>.Shared.Return(arr);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe byte* Utf8Ptr(string? text)
    {
        if (string.IsNullOrEmpty(text)) return null;

        var span = Utf8(text);
        return (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
    }

    public unsafe void Dispose()
    {
        if (disposables is not [])
        {
            for (int i = 0; i < count; i++)
            {
                disposables[i]();
                disposables[i] = null!;
            }

            ArrayPool<Action>.Shared.Return(disposables);
            disposables = Array.Empty<Action>();
        }
    }
}