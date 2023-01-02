namespace Meigs2.Functional.BuiltInTypes;

public static class Enum
{
    public static Option<T> Parse<T>(this string s)
        where T : struct
    {
        return System.Enum.TryParse(s, out T t) ? F.Some(t) : F.Nothing;
    }
}