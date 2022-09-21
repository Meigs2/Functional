using System.Collections;
using System.Collections.Generic;

namespace Functional.Core
{
   public class EmptyList<T> : IEnumerable<T>
   {
      IEnumerator IEnumerable.GetEnumerator() { yield break; }
      IEnumerator<T> IEnumerable<T>.GetEnumerator() { yield break; }
   }
}
