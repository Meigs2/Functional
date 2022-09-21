using System;
using System.Collections.Generic;
using System.Reflection;
using Functional.Core.Enumeration;

namespace Functional.Core;

public static class ValueObject
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

public static class ValueObjectExtensions
{
    public static bool TryGetValues(this Type type, out IEnumerable<object> enums)
    {
        while (type != null)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Enumeration<,>) || type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(FlaggedEnumeration<,>))
            {
                var listPropertyInfo = type.GetProperty("List", BindingFlags.Public | BindingFlags.Static);
                enums = (IEnumerable<object>)listPropertyInfo.GetValue(type, null);
                return true;
            }

            type = type.BaseType;
        }

        enums = null;
        return false;
    }
}
