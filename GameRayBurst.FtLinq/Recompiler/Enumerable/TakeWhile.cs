using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class TakeWhile : TransformMethodBase
    {
        public override bool CanHandle(MethodInfo input)
        {
            return ReflectionUtil.IsMethodFromEnumerable(input, "TakeWhile");
        }

        public override void CreateTransform(RecompilationState state)
        {
            var method = state.CurrentMethodCallExpression;
            var predicate = method.Arguments[1];
            var rewrittenTest = predicate.RewriteCall(state.InputVariable);

            state.Body.Add(Expression.IfThen(Expression.IsFalse(rewrittenTest), Expression.Break(state.BreakLabel)));
        }
    }
}
