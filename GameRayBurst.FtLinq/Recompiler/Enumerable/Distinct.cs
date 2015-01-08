using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class Distinct : TransformMethodBase
    {
        public override bool CanHandle(MethodInfo input)
        {
            return ReflectionUtil.IsMethodFromEnumerable(input, "Distinct")
                   || ReflectionUtil.IsMethodFromFtlEnumerable(input, "Distinct");
        }

        public override void CreateStateVariables(RecompilationState state, IList<ParameterExpression> globalVariables, IList<Expression> globalBody)
        {
            var selector = GetSelector(state);
            var selectorType = selector.GetLambdaReturnType();
            var set = Expression.Variable(typeof (HashSet<>).MakeGenericType(selectorType));
            globalVariables.Add(set);

            var method = state.CurrentMethod;
            var comparerType = typeof(IEqualityComparer<>).MakeGenericType(selectorType);
            var comparerParam = method.GetParameters().FirstOrDefault(p => comparerType.IsAssignableFrom(p.ParameterType));
            var setCtor = set.Type.GetConstructor(comparerParam != null ? new[] {comparerParam.ParameterType} : Type.EmptyTypes);
            var ctorParams = comparerParam != null ? new[] { state.CurrentMethodCallExpression.Arguments[comparerParam.Position] } : new Expression[0];

            globalBody.Add(Expression.Assign(set, Expression.New(setCtor, ctorParams)));
            state.CreateLocal("Distinct", set);
        }

        public override void CreateTransform(RecompilationState state)
        {
            var set = state.GetLocal("Distinct");
            var selector = GetSelector(state);
            var addMethod = set.Type.GetMethod("Add", new[] { selector.GetLambdaReturnType() });

            state.PushBlock(nestedBlock => state.Body.Add(Expression.IfThen(Expression.Call(set, addMethod, selector), nestedBlock)));
        }

        private static Expression GetSelector(RecompilationState state)
        {
            var method = state.CurrentMethod;
            var selectorParam = method.GetParameters().LastOrDefault(p => typeof(Delegate).IsAssignableFrom(p.ParameterType));
            var selector = selectorParam != null ? state.CurrentMethodCallExpression.Arguments[selectorParam.Position] : null;
            return selector != null ? selector.RewriteCall(state.InputVariable) : state.InputVariable;
        }
    }
}
