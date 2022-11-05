using System.Collections.Generic;

namespace Meigs2.Functional
{
   public static class DictionaryExensions
   {
      public static Option<T> Lookup<K, T>(this IDictionary<K, T> dict, K key, bool ignoreCase = false)
      {
         T value;
         return dict.TryGetValue(key, out value) ? F.Some(value) : F.Nothing;
      }
   }
}
