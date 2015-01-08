using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class ToDictionary : AggregationMethodBase
    {
        public override bool CanHandle(MethodInfo input)
        {
            return ReflectionUtil.IsMethodFromEnumerable(input, "ToDictionary");
        }

        public override ParameterExpression CreateAggregationVariable(RecompilationState state, Type aggregationVariableType)
        {
            var method = state.CurrentMethodCallExpression;
            var equalityComparerArg = method.Arguments.Count == 4 ? method.Arguments[3]
                : method.Arguments.Count == 3 ? method.Arguments[2]
                : null;

            var equalityComparerType = equalityComparerArg != null
                ? equalityComparerArg.Type.FindGenericInterfaceInstance(typeof (IEqualityComparer<>))
                : null;

            var ctorExpression = equalityComparerType != null
                ? Expression.New(aggregationVariableType.GetConstructor(new[] {equalityComparerType}), equalityComparerArg)
                : Expression.New(aggregationVariableType);

            var aggregationVar = Expression.Variable(aggregationVariableType);
            state.Variables.Add(aggregationVar);
            state.Body.Add(Expression.Assign(aggregationVar, ctorExpression));
            return aggregationVar;
        }

        public override void AggregateItem(RecompilationState state)
        {
            var method = state.CurrentMethodCallExpression;
            var input = state.InputVariable;
            var aggregationVar = state.AggregationVariable;
            var keySelector = method.Arguments[1];
            var valueSelector = method.Arguments.Count >= 3 ? method.Arguments[2] as LambdaExpression : null;

            var rewrittenKeySelector = keySelector.RewriteCall(input);
            var rewrittenValueSelector = valueSelector != null ? valueSelector.RewriteCall(input) : input;

            var addMethod = aggregationVar.Type.GetMethod("Add");
            state.Body.Add(Expression.Call(aggregationVar, addMethod, rewrittenKeySelector, rewrittenValueSelector));
        }
    }
}
