using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Iterators
{
    internal sealed class EnumerableIterationMethod : IIterationMethod
    {
        public bool CanHandle(Type collectionType)
        {
            return collectionType == typeof(IEnumerable)
                || collectionType.GetInterfaces()
                    .Any(intf =>
                        (intf.IsGenericType && intf.GetGenericTypeDefinition() == typeof (IEnumerable<>))
                        || intf == typeof(IEnumerable));
        }

        public IIterationState CreateIterationState(Expression source, bool checkSequenceEmpty)
        {
            return ExpressionUtil.CreateForEachIterationState(source, checkSequenceEmpty);
        }

        public void CreateIterationBlock(RecompilationState recompilationState, IIterationState state, Expression body)
        {
            if (state.SequenceEmptyVariable != null)
            {
                recompilationState.Variables.Add(state.SequenceEmptyVariable);
            }

            var loop = ExpressionUtil.ForEach(state, body);
            recompilationState.Body.Add(loop);
        }
    }
}
