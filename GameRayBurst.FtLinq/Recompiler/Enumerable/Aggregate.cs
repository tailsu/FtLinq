using System;
using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class Aggregate : AggregationMethodBase
    {
        public override bool CanHandle(MethodInfo input)
        {
            return ReflectionUtil.IsMethodFromEnumerable(input, "Aggregate");
        }

        public override ParameterExpression CreateAggregationVariable(RecompilationState state, Type aggregationVariableType)
        {
            var method = state.CurrentMethodCallExpression;
            return method.Arguments.Count == 2
                ? CreateUnseededAggregationVariable(state)
                : CreateSeededAggregationVariable(state);
        }

        public override void AggregateItem(RecompilationState state)
        {
            var method = state.CurrentMethodCallExpression;
            if (method.Arguments.Count == 2)
                AggregateUnseeded(state);
            else
                AggregateSeeded(state);
        }

        public override void CreateReturnExpression(RecompilationState state)
        {
            var method = state.CurrentMethodCallExpression;

            switch (method.Arguments.Count)
            {
                case 2:
                    var isInitializedVar = state.GetLocal("Aggregate");
                    state.Body.Add(Expression.IfThen(Expression.IsFalse(isInitializedVar),
                        ExpressionUtil.Throw(typeof(InvalidOperationException), "Sequence contains no elements")));
                    state.Body.Add(state.AggregationVariable);
                    break;

                case 3:
                    state.Body.Add(state.AggregationVariable);
                    break;

                case 4:
                    var selector = method.Arguments[3];
                    var rewrittenSelector = selector.RewriteCall(state.AggregationVariable);
                    state.Body.Add(rewrittenSelector);
                    break;
            }
        }

        private static ParameterExpression CreateSeededAggregationVariable(RecompilationState state)
        {
            var method = state.CurrentMethodCallExpression;
            var seed = method.Arguments[1];
            var aggregationVar = Expression.Variable(seed.Type);
            state.Variables.Add(aggregationVar);
            state.Body.Add(Expression.Assign(aggregationVar, seed));
            return aggregationVar;
        }

        private static ParameterExpression CreateUnseededAggregationVariable(RecompilationState state)
        {
            var method = state.CurrentMethodCallExpression;
            var aggregationVar = Expression.Variable(method.Type);
            state.Variables.Add(aggregationVar);
            state.Body.Add(Expression.Assign(aggregationVar, Expression.Default(aggregationVar.Type)));

            var isInitializedVar = Expression.Variable(typeof (bool));
            state.Variables.Add(isInitializedVar);
            state.Body.Add(Expression.Assign(isInitializedVar, Expression.Constant(false)));

            state.CreateLocal("Aggregate", isInitializedVar);

            return aggregationVar;
        }

        private static void AggregateSeeded(RecompilationState state)
        {
            var method = state.CurrentMethodCallExpression;
            var aggregator = method.Arguments[2];
            var rewrittenAggregator = aggregator.RewriteCall(state.AggregationVariable, state.InputVariable);
            state.Body.Add(Expression.Assign(state.AggregationVariable, rewrittenAggregator));
        }

        private static void AggregateUnseeded(RecompilationState state)
        {
            var method = state.CurrentMethodCallExpression;
            var aggregator = method.Arguments[1];
            var rewrittenAggregator = aggregator.RewriteCall(state.AggregationVariable, state.InputVariable);
            var isInitializedVar = state.GetLocal("Aggregate");
            state.Body.Add(Expression.IfThenElse(isInitializedVar,
                Expression.Assign(state.AggregationVariable, rewrittenAggregator),
                Expression.Block(
                    Expression.Assign(isInitializedVar, Expression.Constant(true)),
                    Expression.Assign(state.AggregationVariable, state.InputVariable)
                )));
        }
    }
}
