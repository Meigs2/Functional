using System;

namespace Meigs2.Functional.BuiltInTypes
{
   public static class Date
   {
      public static Option<DateTime> Parse(string s)
      {
         DateTime d;
         return DateTime.TryParse(s, out d) ? F.Some(d) : F.Nothing;
      }
   }
}
