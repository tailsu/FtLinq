using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class OfType : TransformMethodBase
    {
        public override bool CanHandle(MethodInfo input)
        {
            return ReflectionUtil.IsMethodFromEnumerable(input, "OfType");
        }

        public override void CreateTransform(RecompilationState state)
        {
            var inputVar = state.InputVariable;
            var method = state.CurrentMethodCallExpression;
            var resultType = method.Method.GetGenericArguments()[0];
            var newTypeVar = Expression.Variable(resultType);

            if (resultType.IsValueType)
            {
                state.PushBlock(nestedBlock =>
                {
                    var executionExpression =
                        Expression.IfThen(Expression.TypeIs(inputVar, resultType), nestedBlock);
                    state.Body.Add(executionExpression);
                });

                state.Variables.Add(newTypeVar);
                state.Body.Add(Expression.Assign(newTypeVar, Expression.Convert(inputVar, resultType)));
            }
            else
            {
                state.Variables.Add(newTypeVar);
                state.Body.Add(Expression.Assign(newTypeVar, Expression.TypeAs(inputVar, resultType)));
                state.PushBlock(nestedBlock =>
                {
                    var executionExpression =
                        Expression.IfThen(Expression.NotEqual(Expression.Constant(null, resultType), newTypeVar), nestedBlock);
                    state.Body.Add(executionExpression);
                });
            }

            state.InputVariable = newTypeVar;
        }
    }
}
