using System;
using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class Any : AggregationMethodBase
    {
        public override bool CanHandle(MethodInfo input)
        {
            return ReflectionUtil.IsMethodFromEnumerable(input, "Any");
        }

        public override void AggregateItem(RecompilationState state)
        {
            var method = state.CurrentMethodCallExpression;

            if (method.Arguments.Count == 1) // Any()
            {
                state.Body.Add(Expression.Assign(state.AggregationVariable, Expression.Constant(true)));
                state.Body.Add(Expression.Break(state.BreakLabel));
            }
            else // Any(predicate)
            {
                var selector = method.Arguments[1];
                var rewrittenTest = selector.RewriteCall(state.InputVariable);

                state.Body.Add(Expression.IfThen(rewrittenTest,
                    Expression.Block(
                        Expression.Assign(state.AggregationVariable, Expression.Constant(true)),
                        Expression.Break(state.BreakLabel))));
            }
        }
    }
}
