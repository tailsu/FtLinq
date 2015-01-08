using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GameRayBurst.FtLinq.Recompiler;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq
{
    internal class FtlOrderedEnumerable<T> : IFtlOrderedEnumerable<T>, ICollection<T>
    {
        private readonly IEnumerable<T> mySource;
        private readonly IEnumerableOrdering<T> myOrdering;
        private ICollection<T> mySortedSequence;

        public static IFtlOrderedEnumerable<T> CreateOrderedEnumerable<TKey>(IEnumerable<T> source, Expression<Func<T, TKey>> keySelector, IComparer<TKey> comparer, bool descending, IEnumerableOrdering<T> nextOrdering)
        {
            var ordering = new EnumerableOrdering<T, TKey>(keySelector, comparer, descending, nextOrdering);
            return new FtlOrderedEnumerable<T>(source, ordering);
        }

        public IFtlOrderedEnumerable<T> CreateOrderedEnumerable<TKey>(Expression<Func<T, TKey>> keySelector, IComparer<TKey> comparer, bool descending)
        {
            return CreateOrderedEnumerable(mySource, keySelector, comparer, descending, myOrdering);
        }

        public FtlOrderedEnumerable(IEnumerable<T> source, IEnumerableOrdering<T> ordering)
        {
            mySource = source;
            myOrdering = ordering;
        }

        private void DoSort()
        {
            if (mySortedSequence != null)
                return;
            mySortedSequence = myOrdering.Sort(mySource);
        }

        public IEnumerator<T> GetEnumerator()
        {
            DoSort();
            return mySortedSequence.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get
            {
                DoSort();
                return mySortedSequence.Count;
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            DoSort();
            mySortedSequence.CopyTo(array, arrayIndex);
        }

        #region Unsupported methods
        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(T item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public bool IsReadOnly { get { return true; } }
        #endregion
    }

    internal interface IEnumerableOrdering<T>
    {
        ICollection<T> Sort(IEnumerable<T> source);
        void CreateSortingKey(TupleStructBuilder tuple, ref int index);
        void GetKeySelectors(List<LambdaExpression> keySelectors);
        void GetComparers(Dictionary<string, object> comparers, ref int index);
    }

    internal class EnumerableOrdering<T, TKey> : IEnumerableOrdering<T>
    {
        private const string ValueFieldName = "Value";
        private const string FieldNamePrefix = "Field_";

        private readonly Expression<Func<T, TKey>> myKeySelector;
        private readonly IComparer<TKey> myComparer;
        private readonly bool myDescending;
        private readonly IEnumerableOrdering<T> myNextOrdering;

        public EnumerableOrdering(Expression<Func<T, TKey>> keySelector, IComparer<TKey> comparer, bool descending, IEnumerableOrdering<T> nextOrdering)
        {
            if (keySelector == null && typeof(T) != typeof(TKey))
                throw new ArgumentException("Key selector may not be null when key type is different from element type", "keySelector");

            myKeySelector = keySelector;
            if (comparer != null && !ReferenceEquals(comparer, Comparer<TKey>.Default))
                myComparer = comparer;
            myDescending = descending;
            myNextOrdering = nextOrdering;
        }

        public ICollection<T> Sort(IEnumerable<T> source)
        {
            // use a fast path if the ordering is based only on the comparer itself
            if (myNextOrdering == null && !myDescending)
            {
                // use a fast path if the key selector is the identity selector
                if (myKeySelector == null || myKeySelector.IsIdentityTransform())
                {
                    var listSource = source.ToList(); // ToList is faster than ToArray
                    listSource.Sort((IComparer<T>) myComparer);
                    return listSource;
                }
                else // having a single key is a special case, because if TKey is a primitive type, then we'll hit the TrySZSort fast path
                {
                    var arraySource = source.ToArray();

                    var keysVar = Expression.Variable(typeof (TKey[]), "keys");
                    var sourceVar = Expression.Variable(typeof (T[]), "source");
                    var iVar = Expression.Variable(typeof (int), "i");
                    var elementVar = Expression.Variable(typeof (T), "element");
                    var keyProjectionExpr = Expression.Block(new[] { keysVar },
                        Expression.Assign(keysVar, Expression.NewArrayBounds(typeof(TKey), Expression.ArrayLength(sourceVar))),
                        ExpressionUtil.For(new[] {iVar},
                            Expression.Assign(iVar, Expression.Constant(0)),
                            Expression.LessThan(iVar, Expression.ArrayLength(sourceVar)),
                            Expression.PreIncrementAssign(iVar),
                            null, null,
                            Expression.Block(new[] {elementVar},
                                Expression.Assign(elementVar, Expression.ArrayIndex(sourceVar, iVar)),
                                Expression.Assign(Expression.ArrayAccess(keysVar, iVar), myKeySelector.RewriteCall(elementVar)))),
                        keysVar);
                    var keyProjectionLambdaExpr = Expression.Lambda(keyProjectionExpr, sourceVar);
                    
                    var keyProjector = keyProjectionLambdaExpr.Compile() as Func<T[], TKey[]>;
                    var keys = keyProjector(arraySource);
                    Array.Sort(keys, arraySource, 0, arraySource.Length, myComparer);
                    return arraySource;
                }
            }
            else // general case - multiple keys and/or non-trivial ordering
            {
                var sortMethodImpl = CreateSortMethodImpl();
                return sortMethodImpl(source);
            }
        }

        public void CreateSortingKey(TupleStructBuilder tuple, ref int index)
        {
            if (myNextOrdering != null)
                myNextOrdering.CreateSortingKey(tuple, ref index);
            var comparerType = myComparer != null ? myComparer.GetType() : null;
            tuple.AddField(FieldNamePrefix + index, typeof(TKey), comparerType, myDescending ? TupleFieldRole.SortDescending : TupleFieldRole.SortAscending);
            index++;
        }

        public void GetKeySelectors(List<LambdaExpression> keySelectors)
        {
            if (myNextOrdering != null)
                myNextOrdering.GetKeySelectors(keySelectors);
            keySelectors.Add(myKeySelector);
        }

        public void GetComparers(Dictionary<string, object> comparers, ref int index)
        {
            if (myNextOrdering != null)
                myNextOrdering.GetComparers(comparers, ref index);

            if (myComparer != null)
                comparers.Add(FieldNamePrefix + index, myComparer);
            index++;
        }

        private Func<IEnumerable<T>, ICollection<T>> CreateSortMethodImpl()
        {
            var builder = new TupleStructBuilder();
            int keyIndex = 0;
            CreateSortingKey(builder, ref keyIndex);
            builder.AddField(ValueFieldName, typeof(T), null, TupleFieldRole.Value);
            builder.CreateType();

            var keySelectors = new List<LambdaExpression>();
            GetKeySelectors(keySelectors);

            var comparers = new Dictionary<string, object>();
            int compIndex = 0;
            GetComparers(comparers, ref compIndex);

            var comparerInitExpr = builder.CreateComparerInitializationExpression(
                comparers.ToDictionary(p => p.Key, p => (Expression) Expression.Constant(p.Value)));

            var sourceParam = Expression.Variable(typeof(IEnumerable<T>), "source");
            var arraySortExpr = builder.CreateIEnumerableSortExpression(typeof(T), sourceParam,
                ValueFieldName, sourceVar => keySelectors
                    .Select((expr, i) => Tuple.Create(FieldNamePrefix + i, expr.RewriteCall(sourceVar)))
                    .ToDictionary(t => t.Item1, t => t.Item2));

            //TODO: yield values, instead of projecting them into a temporary vector: will save memory

            var lambdaExpression = Expression.Lambda(
                Expression.Block(
                    comparerInitExpr,
                    Expression.Convert(arraySortExpr, typeof(ICollection<T>))
                ), sourceParam);
            var lambda = lambdaExpression.Compile() as Func<IEnumerable<T>, ICollection<T>>;
            return lambda;
        }
    }
}
