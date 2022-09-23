using System.Collections.Generic;

namespace Functional.Core
{
   using static F;

   public static class DictionaryExensions
   {
      public static Option<T> Lookup<K, T>(this IDictionary<K, T> dict, K key, bool ignoreCase = false)
      {
         T value;
         return dict.TryGetValue(key, out value) ? Some(value) : F.Nothing;
      }
   }
}
