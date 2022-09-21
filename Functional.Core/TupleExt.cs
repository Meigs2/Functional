using System;

namespace Functional.Core
{
    public static class TupleExt
   {
      public static R Match<T1, T2, R>(this Tuple<T1, T2> @this
          , Func<T1, T2, R> func) => func(@this.Item1, @this.Item2);
   }
}
