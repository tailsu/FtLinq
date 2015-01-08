using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace GameRayBurst.FtLinq
{
    public interface IFtlOrderedEnumerable<T> : IEnumerable<T>
    {
        IFtlOrderedEnumerable<T> CreateOrderedEnumerable<TKey>(
            Expression<Func<T, TKey>> keySelector, IComparer<TKey> comparer, bool descending);
    }
}