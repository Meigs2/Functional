using System;

namespace Functional.Core
{
   public class Coyo<V, T>
   {
      public V Value { get; }
      public Func<object, T> Func { get; }

      public Coyo(V value, Func<object, T> func)
      {
         Value = value;
         Func = func;
      }
   }

   public static class Coyo
   {
      public static Coyo<V, T> Of<V, T>(V value) => new(value, x => (T)x);

      public static Coyo<V, R> Map<V, T, R>(this Coyo<V, T> @this, Func<T, R> func) =>
          new(@this.Value, x => func(@this.Func(x)));
   }
}
