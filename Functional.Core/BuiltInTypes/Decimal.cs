using System;

namespace Meigs2.Functional.BuiltInTypes;

public static class Decimal
{
    public new static Func<decimal, string> ToString = d => d.ToString();

    public static Option<decimal> Parse(string s)
    {
        decimal result;
        return decimal.TryParse(s, out result)
            ? F.Some(result)
            : F.Nothing;
    }

    public static bool IsOdd(decimal i)
    {
        return i % 2 == 1;
    }

    public static bool IsEven(decimal i)
    {
        return i % 2 == 0;
    }
}