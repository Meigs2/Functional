namespace Meigs2.Functional.BuiltInTypes
{
   public static class Long
   {
      public static Option<long> Parse(string s)
      {
         long result;
         return long.TryParse(s, out result)
            ? F.Some(result) : F.Nothing;
      }
   }
}
