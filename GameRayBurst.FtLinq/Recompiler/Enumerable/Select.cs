using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class Select : TransformMethodBase
    {
        public override bool CanHandle(MethodInfo method)
        {
            return ReflectionUtil.IsMethodFromEnumerable(method, "Select");
        }

        public override void CreateStateVariables(RecompilationState state, IList<ParameterExpression> globalVariables, IList<Expression> globalBody)
        {
            if (state.CurrentMethod.GetParameters().Length == 1)
                return;

            var variable = Expression.Variable(typeof (int));
            globalVariables.Add(variable);
            state.CreateLocal("Select", variable);
            globalBody.Add(Expression.Assign(variable, Expression.Constant(0)));
        }

        public override void CreateTransform(RecompilationState state)
        {
            var selectBody = state.CurrentMethodCallExpression.Arguments[1];
            var outVar = Expression.Variable(selectBody.GetLambdaReturnType());
            state.Variables.Add(outVar);

            if (selectBody.GetLambdaParameterCount() == 1) // select without indexing
            {
                var rewritten = selectBody.RewriteCall(state.InputVariable);
                state.Body.Add(Expression.Assign(outVar, rewritten));
            }
            else // select with indexing
            {
                var counter = state.GetLocal("Select");
                var rewritten = selectBody.RewriteCall(state.InputVariable, counter);
                state.Body.Add(Expression.Assign(outVar, rewritten));
                state.Body.Add(Expression.PreIncrementAssign(counter));
            }

            state.InputVariable = outVar;
        }
    }
}
