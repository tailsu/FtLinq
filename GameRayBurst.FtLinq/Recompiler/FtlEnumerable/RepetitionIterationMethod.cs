using System;
using System.Linq.Expressions;
using GameRayBurst.FtLinq.Recompiler.Iterators;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.FtlEnumerable
{
    internal sealed class RepetitionIterationMethod : IIterationMethod
    {
        private sealed class State : IterationStateBase
        {
            public readonly Expression Source;
            public readonly ParameterExpression Iterator;
            public readonly ParameterExpression Item;

            public State(Expression source, bool createSequenceEmptyVar)
                : base(createSequenceEmptyVar)
            {
                Source = source;
                var repetitionType = source.Type.GetGenericArguments()[0];
                Item = Expression.Variable(repetitionType);
                Iterator = Expression.Variable(typeof (int));
            }

            public override ParameterExpression ItemVariable { get { return Item; } }

            public override Expression Count
            {
                get { return Expression.Field(Source, "Count"); }
            }
        }

        public bool CanHandle(Type input)
        {
            return input.IsGenericType && input.GetGenericTypeDefinition() == typeof (Repetition<>);
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
                    Expression.Equal(Expression.Constant(0), Expression.Field(state.Source, "Count"))));
            }

            var loop = ExpressionUtil.For(new[] {state.Iterator, state.Item},
                Expression.Block(
                    Expression.Assign(state.Iterator, Expression.Constant(0)),
                    Expression.Assign(state.Item, Expression.Field(state.Source, "Value"))),
                Expression.LessThan(state.Iterator, state.Count),
                Expression.PreIncrementAssign(state.Iterator),
                state.ContinueLabel, state.BreakLabel,
                body);

            recompilationState.Body.Add(loop);
        }
    }
}
