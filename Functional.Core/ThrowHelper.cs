using System;
using Meigs2.Functional.Enumeration;

namespace Meigs2.Functional;

static class ThrowHelper
{
    public static void ThrowArgumentNullException(string paramName)
        => throw new ArgumentNullException(paramName);

    public static void ThrowArgumentNullOrEmptyException(string paramName)
        => throw new ArgumentException("Argument cannot be null or empty.", paramName);

    public static void ThrowNameNotFoundException<TObject, TValue>(string name)
        where TObject : Enumeration<TObject, TValue>
        where TValue : IEquatable<TValue>, IComparable<TValue>
        => throw new Exception($"No {typeof(TObject).Name} with Name \"{name}\" found.");

    public static void ThrowValueNotFoundException<TObject, TValue>(TValue value)
        where TObject : Enumeration<TObject, TValue>
        where TValue : IEquatable<TValue>, IComparable<TValue>
        => throw new Exception($"No {typeof(TObject).Name} with Value {value} found.");

    public static void ThrowContainsNegativeValueException<TObject, TValue>()
        where TObject : Enumeration<TObject, TValue>
        where TValue : IEquatable<TValue>, IComparable<TValue>
        => throw new Exception($"The {typeof(TObject).Name} contains negative values other than (-1).");

    public static void ThrowInvalidValueCastException<TObject, TValue>(TValue value)
        where TObject : Enumeration<TObject, TValue>
        where TValue : IEquatable<TValue>, IComparable<TValue>
        => throw new Exception($"The value: {value} input to {typeof(TObject).Name} could not be parsed into an integer value.");

    public static void ThrowNegativeValueArgumentException<TObject, TValue>(TValue value)
        where TObject : Enumeration<TObject, TValue>
        where TValue : IEquatable<TValue>, IComparable<TValue>
        => throw new Exception($"The value: {value} input to {typeof(TObject).Name} was a negative number other than (-1).");
    public static void ThrowNegativeValueArgumentException<TObject, TValue>(int value)
        where TObject : Enumeration<TObject, TValue>
        where TValue : IEquatable<TValue>, IComparable<TValue>
        => throw new Exception($"The value: {value} input to {typeof(TObject).Name} was a negative number other than (-1).");

    public static void ThrowDoesNotContainPowerOfTwoValuesException<TObject, TValue>()
        where TObject : Enumeration<TObject, TValue>
        where TValue : IEquatable<TValue>, IComparable<TValue>
        => throw new Exception($"the {typeof(TObject).Name} does not contain consecutive power of two values.");
}
