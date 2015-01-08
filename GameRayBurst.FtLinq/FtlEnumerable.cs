using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace GameRayBurst.FtLinq
{
    public static class FtlEnumerable
    {
        public static HashSet<T> ToSet<T>(this IEnumerable<T> enumerable)
        {
            return new HashSet<T>(enumerable);
        }

        public static Range Range(int start, int count)
        {
            return new Range(start, count);
        }

        public static Repetition<T> Repeat<T>(T value, int count)
        {
            return new Repetition<T>(value, count);
        }

        public static Generator<T> Generate<T>(Func<T> generator, int count)
        {
            return new Generator<T>(generator, count);
        }

        public static IndexingGenerator<T> Generate<T>(Func<int, T> generator, int count)
        {
            return new IndexingGenerator<T>(generator, count);
        }

        public static IEnumerable<T> Distinct<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> keySelector)
        {
            return Distinct(enumerable, keySelector, EqualityComparer<TKey>.Default);
        }

        public static IEnumerable<T> Distinct<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            var dict = new Dictionary<TKey, T>(comparer);
            foreach (var element in enumerable)
            {
                var key = keySelector(element);
                if (!dict.ContainsKey(key))
                    dict.Add(key, element);
            }
            return dict.Values;
        }

        public static IFtlOrderedEnumerable<T> FtlSort<T>(this IEnumerable<T> source)
        {
            return FtlOrderBy(source, null, Comparer<T>.Default);
        }

        public static IFtlOrderedEnumerable<T> FtlSort<T>(this IEnumerable<T> source, IComparer<T> comparer)
        {
            return FtlOrderBy(source, null, comparer);
        }

        public static IFtlOrderedEnumerable<T> FtlOrderBy<T, TKey>(this IEnumerable<T> source, Expression<Func<T, TKey>> keySelector)
        {
            return FtlOrderBy(source, keySelector, Comparer<TKey>.Default);
        }

        public static IFtlOrderedEnumerable<T> FtlOrderByDescending<T, TKey>(this IEnumerable<T> source, Expression<Func<T, TKey>> keySelector)
        {
            return FtlOrderByDescending(source, keySelector, Comparer<TKey>.Default);
        }

        public static IFtlOrderedEnumerable<T> FtlOrderBy<T, TKey>(this IEnumerable<T> source, Expression<Func<T, TKey>> keySelector, IComparer<TKey> comparer)
        {
            return FtlOrderedEnumerable<T>.CreateOrderedEnumerable(source, keySelector, comparer, false, null);
        }

        public static IFtlOrderedEnumerable<T> FtlOrderByDescending<T, TKey>(this IEnumerable<T> source, Expression<Func<T, TKey>> keySelector, IComparer<TKey> comparer)
        {
            return FtlOrderedEnumerable<T>.CreateOrderedEnumerable(source, keySelector, comparer, true, null);
        }

        public static IFtlOrderedEnumerable<T> ThenBy<T, TKey>(this IFtlOrderedEnumerable<T> source, Expression<Func<T, TKey>> keySelector)
        {
            return ThenBy(source, keySelector, Comparer<TKey>.Default);
        }

        public static IFtlOrderedEnumerable<T> ThenByDescending<T, TKey>(this IFtlOrderedEnumerable<T> source, Expression<Func<T, TKey>> keySelector)
        {
            return ThenByDescending(source, keySelector, Comparer<TKey>.Default);
        }

        public static IFtlOrderedEnumerable<T> ThenBy<T, TKey>(this IFtlOrderedEnumerable<T> source, Expression<Func<T, TKey>> keySelector, IComparer<TKey> comparer)
        {
            return source.CreateOrderedEnumerable(keySelector, comparer, false);
        }

        public static IFtlOrderedEnumerable<T> ThenByDescending<T, TKey>(this IFtlOrderedEnumerable<T> source, Expression<Func<T, TKey>> keySelector, IComparer<TKey> comparer)
        {
            return source.CreateOrderedEnumerable(keySelector, comparer, true);
        }
    }
}
