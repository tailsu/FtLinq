using System;
using System.Linq.Expressions;
using System.Reflection;

namespace GameRayBurst.FtLinq.Recompiler
{
    public interface IAggregationMethod : IMethod<MethodInfo>
    {
        bool SpecialHandlesEmptySequences(MethodInfo method);
        ParameterExpression CreateAggregationVariable(RecompilationState state, Type aggregationVariableType);
        void AggregateItem(RecompilationState state);
        void CreateReturnExpression(RecompilationState state);
    }
}
