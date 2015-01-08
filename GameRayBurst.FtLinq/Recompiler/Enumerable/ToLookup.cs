using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class ToLookup : GroupingAggregationMethod
    {
        public override bool CanHandle(MethodInfo input)
        {
            return ReflectionUtil.IsMethodFromEnumerable(input, "ToLookup");
        }

        public override void CreateReturnExpression(RecompilationState state)
        {
            var dict = state.AggregationVariable;
            var keyValueTypes = dict.Type.GetGenericArguments()[1].GetGenericArguments();
            var lookupType = typeof (Lookup<,>).MakeGenericType(keyValueTypes);
            var lookupCtor = lookupType.GetConstructor(new[] {dict.Type});

            state.Body.Add(Expression.New(lookupCtor, dict));
        }
    }
}
