using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class Where : TransformMethodBase
    {
        public override bool CanHandle(MethodInfo input)
        {
            return ReflectionUtil.IsMethodFromEnumerable(input, "Where");
        }

        public override void CreateStateVariables(RecompilationState state, IList<ParameterExpression> globalVariables, IList<Expression> globalBody)
        {
            var method = state.CurrentMethodCallExpression;
            var lambda = method.Arguments[1];
            if (lambda.GetLambdaParameterCount() != 2)
                return;

            var indexVar = Expression.Variable(typeof (int));
            globalVariables.Add(indexVar);
            state.CreateLocal("Where", indexVar);
            globalBody.Add(Expression.Assign(indexVar, Expression.Constant(0)));
        }

        public override void CreateTransform(RecompilationState state)
        {
            var method = state.CurrentMethodCallExpression;
            var whereBody = method.Arguments[1];
            
            Expression rewrittenTest;
            ParameterExpression counter = null;
            if (whereBody.GetLambdaParameterCount() == 1)
            {
                rewrittenTest = whereBody.RewriteCall(state.InputVariable);
            }
            else
            {
                counter = state.GetLocal("Where");
                rewrittenTest = whereBody.RewriteCall(state.InputVariable, counter);
            }
            
            state.PushBlock(nestedBlock =>
            {
                var executionExpression = Expression.IfThen(rewrittenTest, nestedBlock);
                state.Body.Add(executionExpression);
                if (counter != null)
                    state.Body.Add(Expression.PreIncrementAssign(counter));
            });
        }
    }
}
