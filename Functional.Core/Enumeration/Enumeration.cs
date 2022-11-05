using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Meigs2.Functional.Enumeration;

/// <summary>
/// A base type to use for creating value objects.
/// </summary>
/// <typeparam name="TObject">The type that is inheriting from this class.</typeparam>
/// <typeparam name="TValue">The type of the inner value.</typeparam>
/// <remarks></remarks>
public record Enumeration<TObject, TValue> : ValueObject<TValue> where TObject : Enumeration<TObject, TValue>
    where TValue : IEquatable<TValue>, IComparable<TValue>
{
    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <value>A <see cref="BuiltInTypes.String"/> that is the name of the <see cref="Enumeration{TObject,TValue}"/>.</value>
    public string Name => _name;

    protected readonly string _name;

    /// <summary>
    /// Gets a collection containing all the instances of <see cref="Enumeration{TObject,TValue}"/>.
    /// </summary>
    /// <value>A <see cref="IReadOnlyCollection{TObject}"/> containing all the instances of <see cref="Enumeration{TObject,TValue}"/>.</value>
    /// <remarks>Retrieves all the instances of <see cref="Enumeration{TObject,TValue}"/> referenced by public static read-only fields in the current class or its bases.</remarks>
    public static IReadOnlyCollection<TObject> Values => (IReadOnlyCollection<TObject>)_values.Value.Values;

    protected Enumeration(string name, TValue value) : base(value)
    {
        if (string.IsNullOrEmpty(name)) ThrowHelper.ThrowArgumentNullOrEmptyException(nameof(name));
        if (value == null) ThrowHelper.ThrowArgumentNullException(nameof(value));
        _name = name;
    }

    public static Option<TObject> FromName(string name)
    {
        return string.IsNullOrEmpty(name) ? Option.None : _values.Value.Lookup(name);
    }

    public static Option<TObject> FromValue(TValue value)
    {
        return value is null ? Option.None : _valuesByValue.Value.Lookup(value);
    }

    protected static Option<TObject> AddNewValue(TObject value)
    {
        if (value is null) return Option.None;
        if (_values.Value.ContainsKey(value.Name)) return Option.None;
        _values.Value.TryAdd(value.Name, value);
        _valuesByValue.Value.TryAdd(value.Value, value);
        return value;
    }

    protected static TObject[] GetStaticallyDefinedValues()
    {
        Type baseType = typeof(TObject);
        return Assembly.GetAssembly(baseType).GetTypes().Where(t => baseType.IsAssignableFrom(t))
            .SelectMany(t => t.GetFieldsOfType<TObject>()).OrderBy(t => t.Name).ToArray();
    }

    protected static Lazy<ConcurrentDictionary<string, TObject>> _values =
        new(
            () => new ConcurrentDictionary<string, TObject>(
                StaticallyDefinedValues.Value.ToDictionary(item => item.Name)),
            LazyThreadSafetyMode.ExecutionAndPublication);

    protected static Lazy<ConcurrentDictionary<TValue, TObject>> _valuesByValue =
        new(
            () => new ConcurrentDictionary<TValue, TObject>(
                StaticallyDefinedValues.Value.ToDictionary(item => item.Value, item => item)),
            LazyThreadSafetyMode.ExecutionAndPublication);

    protected static readonly Lazy<TObject[]> StaticallyDefinedValues =
        new(GetStaticallyDefinedValues, LazyThreadSafetyMode.ExecutionAndPublication);

    public virtual bool Equals(Enumeration<TObject, TValue> other)
    {
        if (ReferenceEquals(this, other)) return true;
        return other is not null && _value.Equals(other._value);
    }

    public override string ToString() => _name;
    public override int GetHashCode() => _value.GetHashCode();

    #region Operators

    /// <summary>
    /// Compares this instance to a specified <see cref="Enumeration{TObject,TValue}"/> and returns an indication of their relative values.
    /// </summary>
    /// <param name="other">An <see cref="Enumeration{TObject,TValue}"/> value to compare to this instance.</param>
    /// <returns>A signed number indicating the relative values of this instance and <paramref name="other"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual int CompareTo(Enumeration<TObject, TValue> other) => _value.CompareTo(other._value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Enumeration<TObject, TValue> left, Enumeration<TObject, TValue> right) =>
        left.CompareTo(right) < 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Enumeration<TObject, TValue> left, Enumeration<TObject, TValue> right) =>
        left.CompareTo(right) <= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Enumeration<TObject, TValue> left, Enumeration<TObject, TValue> right) =>
        left.CompareTo(right) > 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Enumeration<TObject, TValue> left, Enumeration<TObject, TValue> right) =>
        left.CompareTo(right) >= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator TValue(Enumeration<TObject, TValue> enumeration) =>
        enumeration is not null ? enumeration._value : default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Enumeration<TObject, TValue>(TValue value) => FromValue(value)
        .ValueOrThrow(new Exception("The value is not a valid enumeration value."));

    #endregion
}

public static class EnumerationExtensions
{
}