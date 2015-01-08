using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;
using System.Linq;

namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class OrderBy : TransformMethodBase
    {
        private const string DescendingSuffix = "Descending";
        public static readonly List<MethodInfo> MethodsSupportingSorting = new List<MethodInfo>(2);

        public override bool CanHandle(MethodInfo input)
        {
            return IsOrderBy(input);
        }

        public override void CreateTransform(RecompilationState state)
        {
            int call = state.CurrentCall - 1;

            while (call > 0 && IsThenBy(state.ExpressionList[call].Method))
                call--;

            if (call == 0)
            {
                var lastMethod = state.ExpressionList[0].Method.GetGenericMethodDefinition();
                if (MethodsSupportingSorting.Contains(lastMethod))
                    return;
            }

            throw new FtlException(state.CurrentMethodCallExpression, "Improper use of OrderBy recompilation. See documentation for details.");
        }

        public override void TransitionState(RecompilationState state)
        {
            state.CurrentCall--;
            while (IsThenBy(state.CurrentMethod))
                state.CurrentCall--;
        }

        private class SortState
        {
            public TupleStructBuilder Builder;
            public ParameterExpression KeyList;
            public Dictionary<string, Expression> KeySelectors;
            public Type ValueType;
            public Dictionary<string, Expression> Comparers;
        }

        public static void CreateSortingVector(Type elementType, RecompilationState state, bool preallocate)
        {
            //TODO: make sure preallocation works really well with aggregation methods
            var sortDescIdx = state.ExpressionList.IndexOf(c => c.Method != null && IsOrderBy(c.Method));
            if (sortDescIdx == -1)
                return;

            // arguments of OrderBy and ThenBy:
            // [0] - IEnumerable<T> source
            // [1] - Func<T, TKey> keySelector
            // [2] - IComparer<TKey> comparer (optional)

            //TODO: add support for FtlOrderBy and the like
            //TODO: add optimized support for identity selectors

            var sortState = new SortState();
            sortState.Builder = new TupleStructBuilder();
            sortState.KeySelectors = new Dictionary<string, Expression>();
            sortState.ValueType = elementType;
            sortState.Comparers = new Dictionary<string, Expression>();
            do
            {
                var call = state.ExpressionList[sortDescIdx].MethodCallExpression;
                var isDescending = call.Method.Name.EndsWith(DescendingSuffix);
                var selector = call.Arguments[1];
                var keyType = selector.GetLambdaReturnType();
                var comparerExpr = call.Arguments.Count >= 3 ? call.Arguments[2] : null;
                var fieldName = "Field_" + sortDescIdx;
                sortState.Builder.AddField(fieldName, keyType,
                    comparerExpr != null ? comparerExpr.Type : null,
                    isDescending ? TupleFieldRole.SortDescending : TupleFieldRole.SortAscending);
                sortState.KeySelectors[fieldName] = selector;

                if (comparerExpr != null)
                    sortState.Comparers[fieldName] = comparerExpr;

                sortDescIdx--;
            } while (IsThenBy(state.ExpressionList[sortDescIdx].Method));

            sortState.Builder.AddField("Value", elementType, null, TupleFieldRole.Value);
            var compositeKeyType = sortState.Builder.CreateType();
            var compositeListType = typeof (List<>).MakeGenericType(compositeKeyType);
            sortState.KeyList = Expression.Variable(compositeListType, "compositeKeys");
            state.Variables.Add(sortState.KeyList);
            state.Body.Add(Expression.Assign(sortState.KeyList, Expression.New(compositeListType)));

            state.StoreContext("Sorting", sortState);
        }

        /// <returns>Returns false if no sorting aggregation is necessary and the input variable should be added straight into the aggregate.</returns>
        public static bool AggregateValue(RecompilationState state)
        {
            var sortState = (SortState) state.GetContext("Sorting");
            if (sortState == null)
                return false;

            var keyType = sortState.Builder.CreateType();
            var keyTuple = Expression.Variable(keyType, "keyTuple");
            state.Variables.Add(keyTuple);
            state.Body.Add(Expression.Assign(keyTuple, Expression.Default(keyType)));
            state.Body.Add(Expression.Assign(Expression.Field(keyTuple, "Value"), state.InputVariable));

            var keySelectors = sortState.KeySelectors.Select(kvp =>
                Expression.Assign(Expression.Field(keyTuple, kvp.Key), kvp.Value.RewriteCall(state.InputVariable)))
                .ToList();
            state.Body.Add(Expression.Block(keySelectors));

            var addMethod = sortState.KeyList.Type.GetMethod("Add");
            state.Body.Add(Expression.Call(sortState.KeyList, addMethod, keyTuple));

            return true;
        }

        public delegate Expression AggregateItemFunc(Expression indexer, Expression sourceElement);

        public static void SortAfterAggregation(RecompilationState state, AggregateItemFunc aggregateItem)
        {
            var sortState = (SortState) state.GetContext("Sorting");
            if (sortState == null)
                return;

            var list = sortState.KeyList;
            var iVar = Expression.Variable(typeof (int), "i");
            var elementType = sortState.Builder.CreateType();
            var valueVar = Expression.Variable(sortState.ValueType, "value");

            var initComparers = sortState.Builder.CreateComparerInitializationExpression(sortState.Comparers);

            var comparerType = typeof (IComparer<>).MakeGenericType(elementType);
            var sortMethod = list.Type.GetMethod("Sort", Type.EmptyTypes);
            var sortCall = Expression.Call(list, sortMethod);

            var listLengthVar = Expression.Variable(typeof(int), "length");
            var projectionLoop = ExpressionUtil.For(
                new[] { iVar, listLengthVar },
                Expression.Block(
                    Expression.Assign(iVar, Expression.Constant(0)),
                    Expression.Assign(listLengthVar, Expression.Property(list, "Count"))),
                Expression.LessThan(iVar, listLengthVar),
                Expression.PreIncrementAssign(iVar),
                null, null,
                Expression.Block(new[] { valueVar },
                    Expression.Assign(valueVar, Expression.Field(Expression.MakeIndex(list, list.Type.GetIndexer(), new[] { iVar }), "Value")),
                    aggregateItem(iVar, valueVar)));

            state.Body.Add(initComparers);
            state.Body.Add(sortCall);
            state.Body.Add(projectionLoop);
        }

        private static bool IsOrderBy(MethodInfo method)
        {
            return IsSortingMethod(method, "OrderBy");
        }

        private static bool IsThenBy(MethodInfo method)
        {
            return IsSortingMethod(method, "ThenBy");
        }

        private static bool IsSortingMethod(MethodInfo method, string name)
        {
            return ReflectionUtil.IsMethodFromEnumerable(method, name)
                   || ReflectionUtil.IsMethodFromEnumerable(method, name + DescendingSuffix);
        }
    }
}
