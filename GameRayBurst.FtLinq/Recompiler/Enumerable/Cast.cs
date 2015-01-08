using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class Cast : TransformMethodBase
    {
        public override bool CanHandle(MethodInfo input)
        {
            return ReflectionUtil.IsMethodFromEnumerable(input, "Cast");
        }

        public override void CreateTransform(RecompilationState state)
        {
            var castType = state.CurrentMethod.GetGenericArguments()[0];
            var outputVariable = Expression.Variable(castType);
            state.Variables.Add(outputVariable);
            state.Body.Add(Expression.Assign(outputVariable, Expression.Convert(state.InputVariable, castType)));
            state.InputVariable = outputVariable;
        }
    }
}
