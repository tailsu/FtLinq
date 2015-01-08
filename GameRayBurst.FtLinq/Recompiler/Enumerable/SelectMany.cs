using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class SelectMany : TransformMethodBase
    {
        public override bool CanHandle(MethodInfo input)
        {
            return ReflectionUtil.IsMethodFromEnumerable(input, "SelectMany");
        }

        public override void CreateStateVariables(RecompilationState state, IList<ParameterExpression> globalVariables, IList<Expression> globalBody)
        {
            var method = state.CurrentMethodCallExpression;
            var selector = method.Arguments[1];
            if (selector.GetLambdaParameterCount() != 2)
                return;

            var counterVar = Expression.Variable(typeof (int));
            globalVariables.Add(counterVar);
            globalBody.Add(Expression.Assign(counterVar, Expression.Constant(0)));
            state.CreateLocal("SelectMany", counterVar);
        }

        public override void CreateTransform(RecompilationState state)
        {
            var input = state.InputVariable;
            var method = state.CurrentMethodCallExpression;
            var selector = method.Arguments[1];
            var projector = method.Arguments.Count == 3 ? method.Arguments[2] : null;
            var selectionInputType = selector.GetLambdaReturnType();

            var iterationMethod = FtlConfiguration.IterationMethods.FindMethod(selectionInputType);
            if (iterationMethod == null)
                throw new FtlException(method, "Unsupported iteration method: {0}");

            var iterationSourceVar = Expression.Variable(selectionInputType);
            state.Variables.Add(iterationSourceVar);

            if (selector.GetLambdaParameterCount() == 1)
            {
                var rewrittenSelector = selector.RewriteCall(input);
                state.Body.Add(Expression.Assign(iterationSourceVar, rewrittenSelector));
            }
            else
            {
                var counterVar = state.GetLocal("SelectMany");
                var rewrittenSelector = selector.RewriteCall(input, counterVar);
                state.Body.Add(Expression.Assign(iterationSourceVar, rewrittenSelector));
                state.Body.Add(Expression.PreIncrementAssign(counterVar));
            }

            var iterationState = iterationMethod.CreateIterationState(iterationSourceVar, state.SequenceEmptyVariable != null);
            state.ContinueLabel = iterationState.ContinueLabel;

            var resultType = projector != null ? projector.GetLambdaReturnType() : selector.GetLambdaReturnType();
            var iterationInputVar = iterationState.ItemVariable;

            Expression rewrittenProjector = null;
            ParameterExpression projectedVar = null;
            if (projector == null)
            {
                state.InputVariable = iterationInputVar;
            }
            else
            {
                projectedVar = Expression.Variable(resultType);
                state.InputVariable = projectedVar;
                rewrittenProjector = projector.RewriteCall(input, iterationInputVar);
            }

            state.PushBlock(nestedBlock =>
                iterationMethod.CreateIterationBlock(state, iterationState,
                    rewrittenProjector == null ? nestedBlock
                    : Expression.Block(new[] { projectedVar },
                        Expression.Assign(projectedVar, rewrittenProjector),
                        nestedBlock)));
        }
    }
}
