using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace GameRayBurst.FtLinq.Recompiler
{
    public interface ITransformMethod : IMethod<MethodInfo>
    {
        void CreateStateVariables(RecompilationState state, IList<ParameterExpression> globalVariables, IList<Expression> globalBody);
        void CreateTransform(RecompilationState state);
        void TransitionState(RecompilationState state);
    }
}
