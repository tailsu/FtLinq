using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal enum ElementSelectorMode
    {
        First, Last, Single, ElementAt
    }

    internal class ElementSelectorImpl : AggregationMethodBase
    {
        private readonly string myMethodName;
        private readonly bool myThrowsOnEmpty;
        private readonly ElementSelectorMode mySelector;
        private readonly bool myCountElements;

        private bool HasCounter { get { return myThrowsOnEmpty || myCountElements; } }

        protected ElementSelectorImpl(string methodName, bool throwsOnEmpty, ElementSelectorMode selector)
        {
            myMethodName = methodName;
            myThrowsOnEmpty = throwsOnEmpty;
            mySelector = selector;
            myCountElements = selector == ElementSelectorMode.Single || selector == ElementSelectorMode.ElementAt;
        }

        public override bool CanHandle(MethodInfo input)
        {
            return ReflectionUtil.IsMethodFromEnumerable(input, myMethodName);
        }

        public override ParameterExpression CreateAggregationVariable(RecompilationState state, Type aggregationVariableType)
        {
            if (HasCounter)
            {
                var counterType = myCountElements ? typeof(int) : typeof(bool);
                var foundAnythingVariable = Expression.Variable(counterType);
                state.Variables.Add(foundAnythingVariable);
                state.CreateLocal(myMethodName, foundAnythingVariable);
                state.Body.Add(Expression.Assign(foundAnythingVariable,
                    mySelector != ElementSelectorMode.ElementAt
                        ? (Expression) Expression.Default(counterType)
                        : Expression.Constant(-1)));
            }

            var variable = Expression.Variable(aggregationVariableType);
            state.Variables.Add(variable);

            if (myThrowsOnEmpty && mySelector == ElementSelectorMode.ElementAt)
            {
                state.Body.Add(Expression.IfThen(
                    Expression.LessThan(GetIndex(state), Expression.Constant(0)),
                    OutOfRangeException()));
            }

            state.Body.Add(Expression.Assign(variable, Expression.Default(aggregationVariableType)));
            return variable;
        }

        public override void AggregateItem(RecompilationState state)
        {
            var method = state.CurrentMethodCallExpression;

            ParameterExpression foundAnythingVar = null;
            Expression foundAnythingUpdater = null;
            if (HasCounter)
            {
                foundAnythingVar = state.GetLocal(myMethodName);
                foundAnythingUpdater = myCountElements
                    ? (Expression) Expression.PreIncrementAssign(foundAnythingVar)
                    : Expression.Assign(foundAnythingVar, Expression.Constant(true));
            }

            var actionBlock = new List<Expression>();
            if (mySelector != ElementSelectorMode.ElementAt)
                actionBlock.Add(Expression.Assign(state.AggregationVariable, state.InputVariable));

            if (mySelector == ElementSelectorMode.Single)
            {
                actionBlock.Add(Expression.IfThen(
                    Expression.Equal(foundAnythingVar, Expression.Constant(1)),
                    ExpressionUtil.Throw(typeof(InvalidOperationException),
                        IsPredicated(state)
                            ? "Sequence contains more than one matching element"
                            : "Sequence contains more than one element")));
            }

            if (foundAnythingUpdater != null)
                actionBlock.Add(foundAnythingUpdater);

            switch (mySelector)
            {
                case ElementSelectorMode.First:
                    actionBlock.Add(Expression.Break(state.BreakLabel));
                    break;
                case ElementSelectorMode.ElementAt:
                    actionBlock.Add(Expression.IfThen(
                        Expression.Equal(foundAnythingVar, GetIndex(state)),
                        Expression.Block(
                            Expression.Assign(state.AggregationVariable, state.InputVariable),
                            Expression.Break(state.BreakLabel))
                            ));
                    break;
            }

            if (!IsPredicated(state))
            {
                foreach (var expr in actionBlock)
                    state.Body.Add(expr);
            }
            else
            {
                var predicate = method.Arguments[1];
                var rewrittenPredicate = predicate.RewriteCall(state.InputVariable);
                state.Body.Add(Expression.IfThen(rewrittenPredicate, Expression.Block(actionBlock)));
            }
        }

        public override void CreateReturnExpression(RecompilationState state)
        {
            if (myThrowsOnEmpty)
            {
                var foundAnythingVar = state.GetLocal(myMethodName);
                var check = mySelector == ElementSelectorMode.ElementAt
                    ? Expression.NotEqual(foundAnythingVar, GetIndex(state))
                    : myCountElements
                        ? Expression.Equal(foundAnythingVar, Expression.Constant(0))
                        : (Expression) Expression.IsFalse(foundAnythingVar);

                var throwExpr = mySelector != ElementSelectorMode.ElementAt
                    ? ExpressionUtil.Throw(typeof(InvalidOperationException),
                        IsPredicated(state)
                            ? "Sequence contains no matching element"
                            : "Sequence contains no elements")
                    : OutOfRangeException();

                state.Body.Add(Expression.IfThen(check, throwExpr));
            }

            base.CreateReturnExpression(state);
        }

        private bool IsPredicated(RecompilationState state)
        {
            return state.CurrentMethodCallExpression.Arguments.Count > 1
                   && mySelector != ElementSelectorMode.ElementAt;
        }

        private static Expression GetIndex(RecompilationState state)
        {
            return state.CurrentMethodCallExpression.Arguments[1];
        }

        private static Expression OutOfRangeException()
        {
            return ExpressionUtil.Throw(typeof (ArgumentOutOfRangeException), "index",
                "Index was out of range. Must be non-negative and less than the size of the collection.");
        }
    }
}
