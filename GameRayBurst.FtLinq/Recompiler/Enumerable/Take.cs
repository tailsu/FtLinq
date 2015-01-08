using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class Take : TransformMethodBase
    {
        public override bool CanHandle(MethodInfo input)
        {
            return ReflectionUtil.IsMethodFromEnumerable(input, "Take");
        }

        public override void CreateStateVariables(RecompilationState state, IList<ParameterExpression> globalVariables, IList<Expression> globalBody)
        {
            var method = state.CurrentMethodCallExpression;
            var counterVar = Expression.Variable(typeof (int));
            globalVariables.Add(counterVar);
            globalBody.Add(Expression.Assign(counterVar, method.Arguments[1]));
            state.CreateLocal("Take", counterVar);
        }

        public override void CreateTransform(RecompilationState state)
        {
            var counterVar = state.GetLocal("Take");

            state.Body.Add(Expression.IfThen(Expression.LessThanOrEqual(
                    Expression.PostDecrementAssign(counterVar), Expression.Constant(0)),
                Expression.Break(state.BreakLabel)));
        }
    }
}
