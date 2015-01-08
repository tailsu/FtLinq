using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler
{
    internal abstract class GroupingAggregationMethod : AggregationMethodBase
    {
        // aggregation variable will be of type IEnumerable<IGrouping<TKey, TValue>>
        public override ParameterExpression CreateAggregationVariable(RecompilationState state, Type aggregationVariableType)
        {
            var method = state.CurrentMethodCallExpression;
            var aggregatorType = MakeAggregatorType(aggregationVariableType);
            Expression aggregatorNew;
            if (method.Arguments.Last() as LambdaExpression == null)
            {
                var comparer = method.Arguments.Last();
                var keyType = aggregatorType.GetGenericArguments()[0];
                var comparerType = typeof (IEqualityComparer<>).MakeGenericType(keyType);
                var ctor = aggregatorType.GetConstructor(new[] {comparerType});
                aggregatorNew = Expression.New(ctor, comparer);
            }
            else
            {
                aggregatorNew = Expression.New(aggregatorType);
            }

            var variable = Expression.Variable(aggregatorType);
            state.Variables.Add(variable);
            state.Body.Add(Expression.Assign(variable, aggregatorNew));
            return variable;
        }

        public override void AggregateItem(RecompilationState state)
        {
            // create an expression for the following pseudo-code snippet:
            //var key = `selector`(`input variable`);
            //Grouping`2 groupList;
            //if (!`aggregation var`.TryGetValue(key, out groupList))
            //{
            //    groupList = new Grouping`2(key);
            //    `aggregation var`.Add(key, groupList);
            //}
            //groupList.Add(`input variable`);

            var aggregatorType = MakeAggregatorType(state.AggregationVariable.Type);
            var aggregatorTypeArgs = aggregatorType.GetGenericArguments();
            var keyType = aggregatorTypeArgs[0];
            var groupingType = aggregatorTypeArgs[1];
            var method = state.CurrentMethodCallExpression;

            var groupKeySelector = method.Arguments[1];
            var rewrittenSelector = groupKeySelector.RewriteCall(state.InputVariable);
            var key = Expression.Variable(keyType);

            state.Variables.Add(key);
            state.Body.Add(Expression.Assign(key, rewrittenSelector));

            var tryGetValueMethod = aggregatorType.GetMethod("TryGetValue");
            var aggregatorAddMethod = aggregatorType.GetMethod("Add");

            var groupVar = Expression.Variable(groupingType);
            state.Variables.Add(groupVar);

            var tryGetValueInvocation = Expression.Call(state.AggregationVariable, tryGetValueMethod, key, groupVar);
            var tryGetBlock = Expression.IfThen(Expression.IsFalse(tryGetValueInvocation),
                Expression.Block(
                    Expression.Assign(groupVar, Expression.New(groupingType.GetConstructor(new[] { keyType }), key)),
                    Expression.Call(state.AggregationVariable, aggregatorAddMethod, key, groupVar)
                    ));
            state.Body.Add(tryGetBlock);

            var groupingAddMethod = groupingType.GetMethod("Add");

            Expression elementValue = state.InputVariable;
            if (method.Arguments.Count >= 3)
            {
                var elementSelector = method.Arguments
                    .Skip(2)
                    .OfType<LambdaExpression>()
                    .FirstOrDefault(arg => arg.Parameters.Count == 1);
                if (elementSelector != null)
                {
                    elementValue = elementSelector.RewriteCall(state.InputVariable);
                }
            }

            state.Body.Add(Expression.Call(groupVar, groupingAddMethod, elementValue));
        }

        private static Type MakeAggregatorType(Type aggregationVariableType)
        {
            var genericArgs = aggregationVariableType.GetGenericArguments();

            Type[] groupingTypeArgs;
            var genericType = aggregationVariableType.GetGenericTypeDefinition();
            if (genericType == typeof(Dictionary<,>))
            {
                groupingTypeArgs = genericArgs[1].GetGenericArguments();
            }
            else if (genericType == typeof(IEnumerable<>))
            {
                groupingTypeArgs = genericArgs[0].GetGenericArguments();
            }
            else if (genericType == typeof(ILookup<,>))
            {
                groupingTypeArgs = genericArgs;
            }
            else
            {
                throw new InvalidOperationException(String.Format("Unsupported grouping type: {0}", aggregationVariableType));
            }

            var keyType = groupingTypeArgs[0];
            var valueType = groupingTypeArgs[1];

            var groupingType = typeof(Grouping<,>).MakeGenericType(keyType, valueType);
            var aggregatorType = typeof(Dictionary<,>).MakeGenericType(keyType, groupingType);
            return aggregatorType;
        }
    }
}