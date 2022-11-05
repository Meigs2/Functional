using System;
using Meigs2.Functional.Enumeration;

namespace Meigs2.Functional;

public static class Value
{
    public static ValueObject<T> From<T>(T value) where T : IEquatable<T>, IComparable<T> => new(value);
}

public record ValueObject<TValue> where TValue : IEquatable<TValue>, IComparable<TValue>
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <value>A <typeparamref name="TValue"/> that is the value of the <see cref="Enumeration{TObject,TValue}"/>.</value>
    public TValue Value => _value;

    protected readonly TValue _value;
    public ValueObject(TValue value) { _value = value; }
    public static implicit operator ValueObject<TValue>(TValue value) => new(value);
}