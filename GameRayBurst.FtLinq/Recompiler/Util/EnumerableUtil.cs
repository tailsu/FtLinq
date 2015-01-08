using System;
using System.Collections.Generic;
using System.Linq;

namespace GameRayBurst.FtLinq.Recompiler.Util
{
    internal static class EnumerableUtil
    {
        public static int IndexOf<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            int i = 0;
            foreach (var t in enumerable)
            {
                if (predicate(t))
                    return i;
                i++;
            }
            return -1;
        }

        public static int LastIndexOf<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            var list = enumerable as IList<T>;
            if (list == null)
                list = enumerable.ToList();

            for (int i = list.Count - 1; i >= 0; --i)
            {
                if (predicate(list[i]))
                    return i;
            }
            return -1;
        }
    }
}
