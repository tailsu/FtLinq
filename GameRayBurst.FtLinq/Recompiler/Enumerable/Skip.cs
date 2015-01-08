using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class Skip : TransformMethodBase
    {
        public override bool CanHandle(MethodInfo input)
        {
            return ReflectionUtil.IsMethodFromEnumerable(input, "Skip");
        }

        public override void CreateStateVariables(RecompilationState state, IList<ParameterExpression> globalVariables, IList<Expression> globalBody)
        {
            var method = state.CurrentMethodCallExpression;
            var skippedCountVar = Expression.Variable(typeof (int));
            globalVariables.Add(skippedCountVar);
            globalBody.Add(Expression.Assign(skippedCountVar, method.Arguments[1]));
            state.CreateLocal("Skip", skippedCountVar);
        }

        public override void CreateTransform(RecompilationState state)
        {
            var countVar = state.GetLocal("Skip");

            state.Body.Add(Expression.IfThen(Expression.GreaterThan(countVar, Expression.Constant(0)),
                Expression.Block(
                    Expression.PreDecrementAssign(countVar),
                    Expression.Continue(state.ContinueLabel)
                )));
        }
    }
}
