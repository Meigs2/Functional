namespace Meigs2.Functional.BuiltInTypes;

public static class Double
{
    public static Option<double> Parse(string s)
    {
        double result;
        return double.TryParse(s, out result)
            ? F.Some(result)
            : F.Nothing;
    }
}