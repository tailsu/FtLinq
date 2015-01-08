using System;
using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class Average : AggregationMethodBase
    {
        public override bool CanHandle(MethodInfo input)
        {
            return ReflectionUtil.IsMethodFromEnumerable(input, "Average");
        }

        public override ParameterExpression CreateAggregationVariable(RecompilationState state, Type aggregationVariableType)
        {
            var counterVar = Expression.Variable(typeof (long));
            state.Variables.Add(counterVar);
            state.CreateLocal("Average", counterVar);
            state.Body.Add(Expression.Assign(counterVar, Expression.Constant(0L)));
            return base.CreateAggregationVariable(state, aggregationVariableType);
        }

        public override void AggregateItem(RecompilationState state)
        {
            var method = state.CurrentMethodCallExpression;
            Expression rewrittenValue;
            var summation = Sum.CreateSummationExpression(state.AggregationVariable, state.InputVariable,
                method.Arguments.Count == 2 ? method.Arguments[1] : null, out rewrittenValue);
            state.Body.Add(summation);

            var counterVar = state.GetLocal("Average");

            if (state.InputVariable.IsNullable())
            {
                state.Body.Add(Expression.IfThen(
                    Expression.Property(rewrittenValue, "HasValue"),
                    Expression.PreIncrementAssign(counterVar)));
            }
            else
            {
                state.Body.Add(Expression.PreIncrementAssign(counterVar));
            }
        }

        public override void CreateReturnExpression(RecompilationState state)
        {
            var method = state.CurrentMethodCallExpression;

            var counterVar = state.GetLocal("Average");

            var nulledType = Nullable.GetUnderlyingType(method.Type);
            if (nulledType == null)
            {
                state.Body.Add(Expression.IfThen(Expression.Equal(counterVar, Expression.Constant(0L)),
                    ExpressionUtil.Throw(typeof(InvalidOperationException),"Sequence contains no elements")));

                state.Body.Add(Expression.Divide(state.AggregationVariable,
                    Expression.Convert(counterVar, state.AggregationVariable.Type)));
            }
            else
            {
                state.Body.Add(Expression.Condition(Expression.NotEqual(counterVar, Expression.Constant(0L)),
                    Expression.Convert(
                        Expression.Divide(Expression.Property(state.AggregationVariable, "Value"),
                            Expression.Convert(counterVar, nulledType)),
                        method.Type),
                        Expression.Constant(null, method.Type)));
            }
        }
    }
}
