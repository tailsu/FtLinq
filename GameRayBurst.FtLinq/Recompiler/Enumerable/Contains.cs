using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class Contains : AggregationMethodBase
    {
        public override bool CanHandle(MethodInfo input)
        {
            return ReflectionUtil.IsMethodFromEnumerable(input, "Contains");
        }

        public override ParameterExpression CreateAggregationVariable(RecompilationState state, Type aggregationVariableType)
        {
            var method = state.CurrentMethodCallExpression;
            Expression equalityComparerArg;
            if (method.Arguments.Count == 3)
            {
                equalityComparerArg = method.Arguments[2];
            }
            else
            {
                var elementType = method.Arguments[0].Type.GetEnumerableElementType();
                var comparer = typeof (EqualityComparer<>).MakeGenericType(elementType);
                var defaultComparerField = comparer.GetProperty("Default");
                equalityComparerArg = Expression.Property(null, defaultComparerField);
            }

            var equalityComparerVar = Expression.Variable(equalityComparerArg.Type);
            state.Variables.Add(equalityComparerVar);
            state.Body.Add(Expression.Assign(equalityComparerVar, equalityComparerArg));
            state.CreateLocal("Contains", equalityComparerVar);

            var testValueArg = method.Arguments[1];
            var testValueVar = Expression.Variable(testValueArg.Type);
            state.Variables.Add(testValueVar);
            state.Body.Add(Expression.Assign(testValueVar, testValueArg));
            state.CreateLocal("Contains_TestValue", testValueVar);

            return base.CreateAggregationVariable(state, aggregationVariableType);
        }

        public override void AggregateItem(RecompilationState state)
        {
            var method = state.CurrentMethodCallExpression;
            var equalityComparerVar = state.GetLocal("Contains");
            var elementType = method.Arguments[0].Type.GetEnumerableElementType();
            var comparerInterface = typeof (IEqualityComparer<>).MakeGenericType(elementType);
            var equalsMethod = equalityComparerVar.Type.FindInterfaceMethod(comparerInterface, "Equals");

            var testValueVar = state.GetLocal("Contains_TestValue");

            var expr =
                Expression.IfThen(Expression.Call(equalityComparerVar, equalsMethod, testValueVar, state.InputVariable),
                    Expression.Block(
                        Expression.Assign(state.AggregationVariable, Expression.Constant(true)),
                        Expression.Break(state.BreakLabel)));

            state.Body.Add(expr);
        }
    }
}
