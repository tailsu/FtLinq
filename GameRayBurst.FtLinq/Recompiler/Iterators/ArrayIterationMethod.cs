using System;
using System.Linq.Expressions;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Iterators
{
    internal sealed class ArrayIterationMethod : IIterationMethod
    {
        private sealed class State : IterationStateBase
        {
            public readonly Expression Source;
            public readonly ParameterExpression Iterator;
            public readonly ParameterExpression ArrayItem;

            public State(Expression source, ParameterExpression iterator, ParameterExpression arrayItem, bool createSequenceEmptyVariable)
                : base(createSequenceEmptyVariable)
            {
                Source = source;
                Iterator = iterator;
                ArrayItem = arrayItem;
            }

            public override ParameterExpression ItemVariable
            {
                get { return ArrayItem; }
            }

            public override Expression Count
            {
                get { return Expression.ArrayLength(Source); }
            }
        }

        public bool CanHandle(Type collectionType)
        {
            return collectionType.IsArray;
        }

        public IIterationState CreateIterationState(Expression source, bool checkSequenceEmpty)
        {
            var elementType = source.Type.GetElementType();
            return new State(source, Expression.Variable(typeof(int)), Expression.Variable(elementType), checkSequenceEmpty);
        }

        public void CreateIterationBlock(RecompilationState recompilationState, IIterationState iterationState, Expression body)
        {
            var state = (State) iterationState;
            var iterator = state.Iterator;
            var arrayLength = state.Count;

            var upperBoundVar = Expression.Variable(typeof (int));
            recompilationState.Variables.Add(upperBoundVar);

            Expression iteratorInitExpr, upperBoundInitExpr;
            recompilationState.CreateIteratedSliceBoundsSettersAndTransitionState(
                iterator, upperBoundVar, arrayLength, out iteratorInitExpr, out upperBoundInitExpr);
            recompilationState.Body.Add(upperBoundInitExpr);

            if (state.SequenceEmptyVariable != null)
            {
                recompilationState.Variables.Add(state.SequenceEmptyVariable);
                recompilationState.Body.Add(Expression.Assign(state.SequenceEmptyVariable,
                    Expression.Equal(Expression.Constant(0), arrayLength)));
            }

            var loop = ExpressionUtil.For(new[] { iterator },
                iteratorInitExpr,
                Expression.LessThan(iterator, upperBoundVar),
                Expression.PreIncrementAssign(iterator),
                state.ContinueLabel, state.BreakLabel,
                Expression.Block(
                    new[] { state.ArrayItem },
                    new[]
                        {
                            Expression.Assign(state.ArrayItem, Expression.ArrayIndex(state.Source, iterator)),
                            body
                        }
                    )
                );

            recompilationState.Body.Add(loop);
        }
    }
}
