using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GameRayBurst.FtLinq.Recompiler.Util
{
    internal static class EmptyEnumerable<TElement>
    {
        public static readonly TElement[] Value = new TElement[0];
    }

    internal sealed class Lookup<TKey, TElement> : ILookup<TKey, TElement>
    {
        private readonly Dictionary<TKey, Grouping<TKey, TElement>> myLookup;

        public Lookup(Dictionary<TKey, Grouping<TKey, TElement>> lookup)
        {
            myLookup = lookup;
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            return myLookup.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains(TKey key)
        {
            return myLookup.ContainsKey(key);
        }

        public int Count
        {
            get { return myLookup.Count; }
        }

        public IEnumerable<TElement> this[TKey key]
        {
            get
            {
                Grouping<TKey, TElement> group;
                if (myLookup.TryGetValue(key, out group))
                    return group;

                return EmptyEnumerable<TElement>.Value;
            }
        }
    }
}
