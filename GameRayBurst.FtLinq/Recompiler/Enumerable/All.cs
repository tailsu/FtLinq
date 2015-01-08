using System;
using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class All : AggregationMethodBase
    {
        public override bool CanHandle(MethodInfo input)
        {
            return ReflectionUtil.IsMethodFromEnumerable(input, "All");
        }

        public override void AggregateItem(RecompilationState state)
        {
            var predicate = state.CurrentMethodCallExpression.Arguments[1];
            var rewrittenPredicate = predicate.RewriteCall(state.InputVariable);

            state.Body.Add(Expression.IfThen(
                Expression.IsFalse(rewrittenPredicate),
                Expression.Block(
                    Expression.Assign(state.AggregationVariable, Expression.Constant(false)),
                    Expression.Break(state.BreakLabel)
                )));
        }

        public override ParameterExpression CreateAggregationVariable(RecompilationState state, Type aggregationVariableType)
        {
            var variable = Expression.Variable(typeof (bool));
            state.Variables.Add(variable);
            state.Body.Add(Expression.Assign(variable, Expression.Constant(true)));
            return variable;
        }
    }
}
