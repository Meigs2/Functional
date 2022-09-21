using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Functional.Core;

public abstract record FlaggedEnumeration<TObject, TValue> : FlaggedEnumerationBase<TObject, TValue>
    where TObject : FlaggedEnumeration<TObject, TValue> where TValue : IEquatable<TValue>, IComparable<TValue>
{
    protected FlaggedEnumeration(string name, TValue value) : base(name, value) { }

    public new static Option<IEnumerable<TObject>> FromName(string name)
    {
        if (string.IsNullOrEmpty(name)) ThrowHelper.ThrowArgumentNullOrEmptyException(nameof(name));
        return FromNameLocal(_values.Value).ToOption();

        IEnumerable<TObject> FromNameLocal(ConcurrentDictionary<string, TObject> dictionary)
        {
            if (!dictionary.TryGetFlagEnumValuesByName<TObject, TValue>(name, out var result))
            {
                ThrowHelper.ThrowNameNotFoundException<TObject, TValue>(name);
            }

            return result;
        }
    }

    public new static Option<IEnumerable<TObject>> FromValue(TValue value)
    {
        if (value == null) return Option.None;
        return GetFlagEnumValues(value, Values);
    }

    public static Option<TObject> GetSingle(TValue value)
    {
        var enumList = Values;
        foreach (var smartFlagEnum in enumList)
        {
            if (smartFlagEnum.Value.Equals(value)) { return smartFlagEnum; }
        }

        return Option.None;
    }

    private static string FormatObjectListString(IEnumerable<TObject> enumInputList)
    {
        var enumList = enumInputList.ToList();
        var sb = new StringBuilder();
        foreach (var smartFlagEnum in enumList)
        {
            sb.Append(smartFlagEnum.Name);
            if (enumList.Last().Name != smartFlagEnum.Name && enumList.Count > 1) { sb.Append(", "); }
        }

        return sb.ToString();
    }

    public override string ToString() => _name;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => _value.GetHashCode();

    public virtual bool Equals(FlaggedEnumeration<TObject, TValue> other)
    {
        // check if same instance
        if (ReferenceEquals(this, other)) return true;

        // it's not same instance so 
        // check if it's not null and is same value
        return other is not null && _value.Equals(other._value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual int CompareTo(FlaggedEnumeration<TObject, TValue> other) => _value.CompareTo(other._value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(FlaggedEnumeration<TObject, TValue> left,
        FlaggedEnumeration<TObject, TValue> right) => left.CompareTo(right) < 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(FlaggedEnumeration<TObject, TValue> left,
        FlaggedEnumeration<TObject, TValue> right) => left.CompareTo(right) <= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(FlaggedEnumeration<TObject, TValue> left,
        FlaggedEnumeration<TObject, TValue> right) => left.CompareTo(right) > 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(FlaggedEnumeration<TObject, TValue> left,
        FlaggedEnumeration<TObject, TValue> right) => left.CompareTo(right) >= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator TValue(FlaggedEnumeration<TObject, TValue> flaggedEnumeration) =>
        flaggedEnumeration._value;
}

public static class FlaggedEnumerationExtensions
{
    public static bool IsFlaggedEnumeration(this Type type) => IsFlaggedEnumeration(type, out var _);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="genericArguments"></param>
    /// <returns></returns>
    public static bool IsFlaggedEnumeration(this Type type, out Type[] genericArguments)
    {
        if (type is null || type.IsAbstract || type.IsGenericTypeDefinition)
        {
            genericArguments = null;
            return false;
        }

        do
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(FlaggedEnumeration<,>))
            {
                genericArguments = type.GetGenericArguments();
                return true;
            }

            type = type.BaseType;
        } while (!(type is null));

        genericArguments = null;
        return false;
    }

    public static bool TryGetFlagEnumValuesByName<TObject, TValue>(this ConcurrentDictionary<string, TObject> dictionary,
        string names, out IEnumerable<TObject> outpuTObjects) where TObject : FlaggedEnumeration<TObject, TValue>
        where TValue : IEquatable<TValue>, IComparable<TValue>
    {
        var outputList = new List<TObject>(dictionary.Count);
        var commaSplitNameList = names.Replace(" ", "").Trim().Split(',');
        Array.Sort(commaSplitNameList);
        foreach (var enumValue in dictionary.Values)
        {
            var result = Array.BinarySearch(commaSplitNameList, enumValue.Name);
            if (result >= 0) { outputList.Add(enumValue); }
        }

        if (!outputList.Any())
        {
            outpuTObjects = null;
            return false;
        }

        outpuTObjects = outputList.ToList();
        return true;
    }
}

public record DynamicFlaggedEnumeration<TObject, TValue> : FlaggedEnumeration<TObject, TValue>
    where TObject : FlaggedEnumeration<TObject, TValue> where TValue : IEquatable<TValue>, IComparable<TValue>
{
    public DynamicFlaggedEnumeration(string name, TValue value) : base(name, value)
    {
    }
} 
