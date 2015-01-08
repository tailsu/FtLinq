using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class GroupBy : GroupingAggregationMethod
    {
        public override bool CanHandle(MethodInfo input)
        {
            return ReflectionUtil.IsMethodFromEnumerable(input, "GroupBy");
        }

        public override void CreateReturnExpression(RecompilationState state)
        {
            state.Body.Add(Expression.Property(state.AggregationVariable, "Values"));
        }
    }
}
