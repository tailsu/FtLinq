using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.FtlEnumerable
{
    internal sealed class ToSet : AggregationMethodBase
    {
        public override bool CanHandle(MethodInfo input)
        {
            return ReflectionUtil.IsMethodFromFtlEnumerable(input, "ToSet");
        }

        public override void AggregateItem(RecompilationState state)
        {
            var addMethod = state.AggregationVariable.Type.GetMethod("Add");
            state.Body.Add(Expression.Call(state.AggregationVariable, addMethod, state.InputVariable));
        }
    }
}
