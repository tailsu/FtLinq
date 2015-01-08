using System;
using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class Sum : AggregationMethodBase
    {
        public override bool CanHandle(MethodInfo method)
        {
            return method.DeclaringType == typeof (System.Linq.Enumerable) && method.Name == "Sum";
        }

        public override ParameterExpression CreateAggregationVariable(RecompilationState state, Type aggregationVariableType)
        {
            var variable = Expression.Variable(aggregationVariableType);
            state.Variables.Add(variable);
            
            var underlyingType = Nullable.GetUnderlyingType(variable.Type);
            state.Body.Add(Expression.Assign(variable,
                underlyingType == null
                ? (Expression) Expression.Constant(Convert.ChangeType(0, variable.Type))
                : Expression.New(variable.Type.GetConstructor(new[] { underlyingType }),
                    Expression.Constant(Convert.ChangeType(0, underlyingType)))));

            return variable;
        }

        public override void AggregateItem(RecompilationState state)
        {
            var method = state.CurrentMethodCallExpression;
            Expression value;
            var summation = CreateSummationExpression(state.AggregationVariable, state.InputVariable,
                method.Arguments.Count == 2 ? method.Arguments[1] : null, out value);

            state.Body.Add(summation);
        }

        public static Expression CreateSummationExpression(ParameterExpression aggregationVar, ParameterExpression inputVar,
            Expression selector, out Expression rewrittenValue)
        {
            rewrittenValue = selector != null
                ? selector.RewriteCall(inputVar)
                : inputVar;

            return Expression.Assign(
                aggregationVar,
                ExpressionUtil.CreateNullableBinaryExpression(
                    aggregationVar,
                    rewrittenValue.Type == aggregationVar.Type ? rewrittenValue : Expression.Convert(rewrittenValue, aggregationVar.Type),
                    ExpressionType.Add, true));
        }
    }
}
