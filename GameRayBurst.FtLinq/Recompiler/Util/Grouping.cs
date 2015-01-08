using System.Collections.Generic;
using System.Linq;

namespace GameRayBurst.FtLinq.Recompiler.Util
{
    internal sealed class Grouping<TKey, TValue> : List<TValue>, IGrouping<TKey, TValue>
    {
        private readonly TKey myKey;

        // this constructor is accessed thru reflection in class GroupBy
        public Grouping(TKey key)
        {
            myKey = key;
        }

        public TKey Key
        {
            get { return myKey; }
        }
    }
}
