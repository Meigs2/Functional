using System;
using Meigs2.Functional.Enumeration;

namespace Meigs2.Functional;

public static class Value
{
    public static ValueObject<T> From<T>(T value)
        where T : IEquatable<T>, IComparable<T> => new(value);
}

public record ValueObject<TValue>
    where TValue : IEquatable<TValue>, IComparable<TValue>
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <value>A <typeparamref name="TValue"/> that is the value of the <see cref="Enumeration{TObject,TValue}"/>.</value>
    public TValue Value => _value;

    protected readonly TValue _value;
    public ValueObject(TValue value) { _value = value; }
    public static implicit operator ValueObject<TValue>(TValue value) => new(value);

    public int CompareTo(ValueObject<TValue>? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return _value.CompareTo(other._value);
    }
}

public record ValueObject<TValueObject, TValue> : ValueObject<TValue>, IComparable<TValueObject>
    where TValueObject : ValueObject<TValueObject, TValue>, IEquatable<TValueObject>
    where TValue : IEquatable<TValue>, IComparable<TValue>
{
    public ValueObject(TValue value) : base(value) { }
    public static implicit operator ValueObject<TValueObject, TValue>(TValue value) => new(value);
    public static implicit operator ValueObject<TValueObject, TValue>(TValueObject value) => new(value.Value);

    public int CompareTo(TValueObject? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return _value.CompareTo(other._value);
    }
}
