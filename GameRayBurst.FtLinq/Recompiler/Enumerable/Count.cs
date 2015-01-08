using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal class CountImpl : AggregationMethodBase
    {
        private readonly string myCountMethod;

        protected CountImpl(string countMethod)
        {
            myCountMethod = countMethod;
        }

        public override bool CanHandle(MethodInfo input)
        {
            return ReflectionUtil.IsMethodFromEnumerable(input, myCountMethod);
        }

        public override void AggregateItem(RecompilationState state)
        {
            var method = state.CurrentMethodCallExpression;

            if (method.Arguments.Count == 1)
            {
                state.Body.Add(Expression.PreIncrementAssign(state.AggregationVariable));
            }
            else
            {
                var predicate = method.Arguments[1];
                var rewrittenTest = predicate.RewriteCall(state.InputVariable);

                state.Body.Add(Expression.IfThen(rewrittenTest, Expression.PreIncrementAssign(state.AggregationVariable)));
            }
        }
    }

    internal sealed class Count : CountImpl
    {
        public Count() : base("Count")
        {}
    }

    internal sealed class LongCount : CountImpl
    {
        public LongCount() : base("LongCount")
        {}
    }
}
