using System;
using System.Linq.Expressions;
using GameRayBurst.FtLinq.Recompiler.Iterators;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.FtlEnumerable
{
    internal sealed class GeneratorIterationMethod : IIterationMethod
    {
        private sealed class State : IterationStateBase
        {
            public readonly ParameterExpression Item;
            public readonly ParameterExpression Iterator;
            public readonly Expression Source;

            public State(Expression source, bool createSequenceEmptyVar)
                : base(createSequenceEmptyVar)
            {
                Source = source;
                Iterator = Expression.Variable(typeof (int));
                Item = Expression.Variable(Expression.Field(source, "GeneratorFunction").Type.GetGenericArguments()[0]);
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
            return input.IsGenericType && (input.GetGenericTypeDefinition() == typeof (Generator<>)
                || input.GetGenericTypeDefinition() == typeof (IndexingGenerator<>));
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

            var generatorParams = state.Source.Type.GetGenericTypeDefinition() == typeof (Generator<>)
                ? new Expression[0]
                : new Expression[] {state.Iterator};

            var generatorInvoke = Expression.Invoke(
                Expression.Field(state.Source, "GeneratorFunction"),
                generatorParams);

            var loop = ExpressionUtil.For(new[] {state.Iterator},
                Expression.Assign(state.Iterator, Expression.Constant(0)),
                Expression.LessThan(state.Iterator, state.Count),
                Expression.PreIncrementAssign(state.Iterator),
                state.ContinueLabel, state.BreakLabel,
                Expression.Block(new[] {state.Item},
                    Expression.Assign(state.Item, generatorInvoke),
                    body));

            recompilationState.Body.Add(loop);
        }
    }
}
