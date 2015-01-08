using System;
using System.Linq.Expressions;

namespace GameRayBurst.FtLinq.Recompiler
{
    public interface IIterationState
    {
        Expression Count { get; }
        ParameterExpression ItemVariable { get; }
        ParameterExpression SequenceEmptyVariable { get; }
        LabelTarget BreakLabel { get; }
        LabelTarget ContinueLabel { get; }
    }

    public interface IIterationMethod : IMethod<Type>
    {
        IIterationState CreateIterationState(Expression source, bool checkSequenceEmpty);
        void CreateIterationBlock(RecompilationState recompilationState, IIterationState state, Expression body);
    }
}
