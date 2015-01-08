using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace GameRayBurst.FtLinq.Recompiler
{
    public abstract class TransformMethodBase : ITransformMethod
    {
        public abstract bool CanHandle(MethodInfo input);

        public abstract void CreateTransform(RecompilationState state);

        public virtual void CreateStateVariables(RecompilationState state, IList<ParameterExpression> globalVariables, IList<Expression> globalBody)
        {
        }

        public virtual void TransitionState(RecompilationState state)
        {
            state.CurrentCall--;
        }
    }
}
