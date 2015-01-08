using System;
using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;
using System.Linq;

namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class ToList : AggregationMethodBase
    {
        static ToList()
        {
            OrderBy.MethodsSupportingSorting.Add(typeof(System.Linq.Enumerable).GetMethod("ToList"));
        }

        public override bool CanHandle(MethodInfo method)
        {
            return ReflectionUtil.IsMethodFromEnumerable(method, "ToList");
        }

        public override ParameterExpression CreateAggregationVariable(RecompilationState state, Type aggregationVariableType)
        {
            var preallocate = state.GetOptions(e => e.PreallocateOutputFromInputLength()).Any();

            var elementType = aggregationVariableType.GetGenericArguments()[0];
            OrderBy.CreateSortingVector(elementType, state, preallocate);

            if (!preallocate)
                return base.CreateAggregationVariable(state, aggregationVariableType);

            var list = CreateDefaultAggregationVariable(state, aggregationVariableType);

            var count = state.IterationState.Count;
            if (count == null)
                throw new FtlException(state.CurrentMethodCallExpression, "Cannot preallocate output because the input length is not known.");
            var ctor = aggregationVariableType.GetConstructor(new[] {typeof (int)});
            var init = Expression.Assign(list, Expression.New(ctor, count));
            state.Body.Add(init);
            
            return list;
        }

        public override void AggregateItem(RecompilationState state)
        {
            if (!OrderBy.AggregateValue(state))
            {
                state.Body.Add(CreateAddExpression(state, state.InputVariable));
            }
        }

        private static Expression CreateAddExpression(RecompilationState state, Expression input)
        {
            var addMethod = state.AggregationVariable.Type.GetMethod("Add");
            return Expression.Call(state.AggregationVariable, addMethod, input);
        }

        public override void CreateReturnExpression(RecompilationState state)
        {
            OrderBy.SortAfterAggregation(state, (indexer, sourceElement) => CreateAddExpression(state, sourceElement));
            base.CreateReturnExpression(state);
        }
    }
}
