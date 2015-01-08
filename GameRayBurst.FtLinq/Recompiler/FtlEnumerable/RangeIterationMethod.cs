using System;
using System.Linq.Expressions;
using GameRayBurst.FtLinq.Recompiler.Iterators;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.FtlEnumerable
{
    internal sealed class RangeIterationMethod : IIterationMethod
    {
        private sealed class State : IterationStateBase
        {
            public readonly ParameterExpression Item;
            public readonly Expression Source;
            public readonly ParameterExpression Iterator;

            public State(Expression source, bool createSequenceEmptyVar)
                : base(createSequenceEmptyVar)
            {
                Source = source;
                Item = Expression.Variable(typeof (int));
                Iterator = Expression.Variable(typeof (int));
            }

            public override ParameterExpression ItemVariable
            {
                get { return Item; }
            }

            public override Expression Count
            {
                get { return Expression.Field(Source, "Count"); }
            }
        }

        public bool CanHandle(Type input)
        {
            return input == typeof (Range);
        }

        public IIterationState CreateIterationState(Expression source, bool checkSequenceEmpty)
        {
            return new State(source, checkSequenceEmpty);
        }

        public void CreateIterationBlock(RecompilationState recompilationState, IIterationState iterationState, Expression body)
        {
            var state = (State) iterationState;

            if (state.SequenceEmptyVariable != null)
            {
                recompilationState.Variables.Add(state.SequenceEmptyVariable);
                recompilationState.Body.Add(Expression.Assign(state.SequenceEmptyVariable,
                    Expression.Equal(Expression.Constant(0), state.Count)));
            }

            var loop = ExpressionUtil.For(new[] { state.Item, state.Iterator },
                Expression.Assign(state.Iterator, Expression.Constant(0)),
                Expression.LessThan(state.Iterator, state.Count),
                Expression.PreIncrementAssign(state.Iterator),
                state.ContinueLabel, state.BreakLabel,
                Expression.Block(
                    Expression.Assign(state.Item, Expression.Add(state.Iterator, Expression.Field(state.Source, "Start"))),
                    body));

            recompilationState.Body.Add(loop);
        }
    }
}
