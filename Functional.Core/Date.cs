using System;

namespace Functional.Core
{
   using static F;

   public static class Date
   {
      public static Option<DateTime> Parse(string s)
      {
         DateTime d;
         return DateTime.TryParse(s, out d) ? Some(d) : F.Nothing;
      }
   }
}
