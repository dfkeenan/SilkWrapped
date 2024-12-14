namespace SilkWrapped.WebGPU;
public ref partial struct NullableRef<T> 
    where T : allows ref struct
{
    private readonly bool hasValue; 
    internal T value; 

    public NullableRef(T value)
    {
        this.value = value;
        hasValue = true;
    }

    public readonly bool HasValue
    {
        get => hasValue;
    }

    public readonly T Value
    {
        get
        {
            if (!hasValue)
            {
                throw new InvalidOperationException("Has No Value");
            }
            return value;
        }
    }

    public readonly T GetValueOrDefault() => value;

    public readonly T GetValueOrDefault(T defaultValue) =>
        hasValue ? value : defaultValue;

    public static implicit operator NullableRef<T>(T value) =>
        new NullableRef<T>(value);

    public static explicit operator T(NullableRef<T> value) => value!.Value;
}
