using System;
using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal class MinMax : AggregationMethodBase
    {
        private readonly string myEnumerableMethodName;
        private readonly string myInitializerFieldName;
        private readonly ExpressionType myComparisonType;

        protected MinMax(string enumerableMethodName, string initializerFieldName, ExpressionType aggregateVsCurrentComparisonType)
        {
            myEnumerableMethodName = enumerableMethodName;
            myInitializerFieldName = initializerFieldName;
            myComparisonType = aggregateVsCurrentComparisonType;
        }

        public override bool SpecialHandlesEmptySequences(MethodInfo method)
        {
            var enumerable =  method.GetParameters()[0].ParameterType;
            var enumeratedType = enumerable.GetGenericArguments()[0];
            var underlyingType = Nullable.GetUnderlyingType(enumeratedType);
            return underlyingType == null;
        }

        public override bool CanHandle(MethodInfo method)
        {
            return method.DeclaringType == typeof(System.Linq.Enumerable) && method.Name == myEnumerableMethodName;
        }

        public override ParameterExpression CreateAggregationVariable(RecompilationState state, Type aggregationVariableType)
        {
            var variable = Expression.Variable(aggregationVariableType);
            state.Variables.Add(variable);

            var underlyingType = Nullable.GetUnderlyingType(variable.Type);
            state.Body.Add(Expression.Assign(variable,
                underlyingType == null
                ? (Expression) Expression.Field(null, aggregationVariableType, myInitializerFieldName)
                : Expression.Constant(null, variable.Type)));

            return variable;
        }

        public override void AggregateItem(RecompilationState state)
        {
            var methodCall = state.CurrentMethodCallExpression;

            ParameterExpression value = state.InputVariable;

            // apply selector lambda if the appropriate overload was called
            if (methodCall.Arguments.Count == 2)
            {
                var selector = methodCall.Arguments[1].RewriteCall(state.InputVariable);

                value = Expression.Variable(selector.Type);
                state.Variables.Add(value);
                state.Body.Add(Expression.Assign(value, selector));
            }

            // create 'if' block that updates the aggregation variable
            var underlyingType = Nullable.GetUnderlyingType(value.Type);
            if (underlyingType == null) // non-nullable case
            {
                state.Body.Add(Expression.IfThen(
                    Expression.MakeBinary(myComparisonType, state.AggregationVariable, value),
                    Expression.Assign(state.AggregationVariable, value)));
            }
            else // nullable case
            {
                // pseudocode: if (value.HasValue && (!aggr.HasValue || aggr.Value > value.Value)) { aggr = value; }
                state.Body.Add(Expression.IfThen(
                    Expression.AndAlso(
                        Expression.Property(value, "HasValue"),
                        Expression.OrElse(
                            Expression.IsFalse(Expression.Property(state.AggregationVariable, "HasValue")),
                            Expression.MakeBinary(myComparisonType,
                                                  Expression.Property(state.AggregationVariable, "Value"),
                                                  Expression.Property(value, "Value")))),
                    Expression.Assign(state.AggregationVariable, value)
                                   ));
            }
        }

        public override void CreateReturnExpression(RecompilationState state)
        {
            if (Nullable.GetUnderlyingType(state.AggregationVariable.Type) == null)
            {
                state.Body.Add(
                    Expression.IfThen(state.SequenceEmptyVariable,
                    ExpressionUtil.Throw(typeof(InvalidOperationException), "Sequence contains no elements")));
            }

            base.CreateReturnExpression(state);
        }
    }

    internal sealed class Min : MinMax
    {
        public Min() : base("Min", "MaxValue", ExpressionType.GreaterThan)
        {}
    }

    internal sealed class Max : MinMax
    {
        public Max() : base("Max", "MinValue", ExpressionType.LessThan)
        {}
    }
}
