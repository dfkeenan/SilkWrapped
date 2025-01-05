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

    public unsafe TTo* Pin<TFrom, TTo>(TFrom[] value)
        where TFrom : unmanaged
        where TTo : unmanaged
    {

        var memory = new Memory<TFrom>(value);
        var handle = memory.Pin();

        var dispose = () =>
        {
            handle.Dispose();
        };

        AddDisposable(dispose);

        return (TTo*)handle.Pointer;
    }


    public unsafe Span<T> RentSpan<T>(int length)
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

    public unsafe T* RentPtr<T>(int length)
        where T : unmanaged
    {
        var span = RentSpan<T>(length);
        return AsPointer(span);
    }

    public unsafe T* RentPtr<T>(ref readonly T value)
        where T : unmanaged
    {
        var owner = MemoryPool<T>.Shared.Rent(1);

        var memory = owner.Memory;
        memory.Span[0] = value;

        var handle = memory.Pin();
        var ptr = (T*)handle.Pointer;

        var dispose = () =>
        {
            handle.Dispose();
            owner.Dispose();
        };

        AddDisposable(dispose);

        return ptr;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T* AsPointer<T>(Span<T> span)
        where T : unmanaged
    {
        return (T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
    }

    public unsafe Span<byte> RentUtf8(string? text)
    {
        if (string.IsNullOrEmpty(text)) return Span<byte>.Empty;

        byte[]? arr = null;

        try
        {
            Span<byte> bytes = text.Length <= 256 ? stackalloc byte[text.Length] : arr = ArrayPool<byte>.Shared.Rent(text.Length);

            var length = Encoding.UTF8.GetBytes(text, bytes);
            var utf8Bytes = RentSpan<byte>(length + 1);
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
    public unsafe byte* RentUtf8Ptr(string? text)
    {
        if (string.IsNullOrEmpty(text)) return null;

        var span = RentUtf8(text);
        return (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddDisposable(Action dispose)
    {
        if (count == disposables.Length)
        {
            var newDisposables = ArrayPool<Action>.Shared.Rent(GetNewCapacity(count + 1));
            disposables.CopyTo(newDisposables, 0);

            if (disposables.Length > 0)
            {
                ArrayPool<Action>.Shared.Return(disposables);
            }

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