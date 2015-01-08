using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class SkipWhile : TransformMethodBase
    {
        public override bool CanHandle(MethodInfo input)
        {
            return ReflectionUtil.IsMethodFromEnumerable(input, "SkipWhile");
        }

        public override void CreateStateVariables(RecompilationState state, IList<ParameterExpression> globalVariables, IList<Expression> globalBody)
        {
            var isSkippingVar = Expression.Variable(typeof (bool));
            globalVariables.Add(isSkippingVar);
            state.CreateLocal("SkipWhile", isSkippingVar);
            globalBody.Add(Expression.Assign(isSkippingVar, Expression.Constant(true)));
        }

        public override void CreateTransform(RecompilationState state)
        {
            var method = state.CurrentMethodCallExpression;
            var predicate = method.Arguments[1];
            var rewrittenTest = predicate.RewriteCall(state.InputVariable);
            var isSkippingVar = state.GetLocal("SkipWhile");

            state.Body.Add(Expression.IfThen(
                Expression.AndAlso(isSkippingVar, rewrittenTest),
                Expression.Continue(state.ContinueLabel)));
            state.Body.Add(Expression.Assign(isSkippingVar, Expression.Constant(false)));
        }
    }
}
