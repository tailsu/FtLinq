using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Iterators
{
    internal sealed class ListIterationMethod : IIterationMethod
    {
        private sealed class State : IterationStateBase
        {
            public readonly Expression Source;
            public readonly ParameterExpression Iterator;
            public readonly ParameterExpression ListItem;

            public State(Expression source, ParameterExpression iterator, ParameterExpression listItem, bool createSequenceEmptyVar)
                : base(createSequenceEmptyVar)
            {
                Source = source;
                Iterator = iterator;
                ListItem = listItem;
            }

            public override ParameterExpression ItemVariable
            {
                get { return ListItem; }
            }

            public override Expression Count
            {
                get
                {
                    var collectionType = Source.Type.FindGenericInterfaceInstance(typeof(ICollection<>));
                    return Expression.Property(Source, collectionType.GetProperty("Count"));
                }
            }
        }

        private static Type GetListType(Type collectionType)
        {
            return
                collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() == typeof(IList<>)
                ? collectionType
                : collectionType.GetInterfaces().FirstOrDefault(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>));
        }

        public bool CanHandle(Type collectionType)
        {
            return GetListType(collectionType) != null;
        }

        public IIterationState CreateIterationState(Expression source, bool checkSequenceEmpty)
        {
            var elementType = GetListType(source.Type).GetGenericArguments()[0];
            return new State(source, Expression.Variable(typeof(int)), Expression.Variable(elementType), checkSequenceEmpty);
        }

        public void CreateIterationBlock(RecompilationState recompilationState, IIterationState iterationState, Expression body)
        {
            var state = (State) iterationState;
            var iterator = state.Iterator;
            var source = state.Source;
            var listItem = state.ListItem;

            var countVar = Expression.Variable(typeof (int));
            recompilationState.Variables.Add(countVar);
            var countProp = state.Count;

            Expression iteratorInitExpr, upperBoundInitExpr;
            recompilationState.CreateIteratedSliceBoundsSettersAndTransitionState(
                iterator, countVar, countProp, out iteratorInitExpr, out upperBoundInitExpr);
            recompilationState.Body.Add(upperBoundInitExpr);

            if (state.SequenceEmptyVariable != null)
            {
                recompilationState.Variables.Add(state.SequenceEmptyVariable);
                recompilationState.Body.Add(Expression.Assign(state.SequenceEmptyVariable,
                    Expression.Equal(Expression.Constant(0), countVar)));
            }

            var indexer = source.Type.GetIndexer();
            var loop = ExpressionUtil.For(new[] { iterator },
                iteratorInitExpr,
                Expression.LessThan(iterator, countVar),
                Expression.PreIncrementAssign(iterator),
                state.ContinueLabel, state.BreakLabel,
                Expression.Block(
                    new[] { listItem },
                    new[]
                        {
                            Expression.Assign(listItem, Expression.MakeIndex(source, indexer, new[] {iterator})),
                            body
                        }
                    ));

            recompilationState.Body.Add(loop);
        }
    }
}
