namespace Meigs2.Functional.BuiltInTypes
{
   public static class Int
   {
      public static Option<int> Parse(string s)
      {
         int result;
         return int.TryParse(s, out result)
            ? F.Some(result) : F.Nothing;
      }

      public static bool IsOdd(int i) => i % 2 == 1;

      public static bool IsEven(int i) => i % 2 == 0;
   }
}
