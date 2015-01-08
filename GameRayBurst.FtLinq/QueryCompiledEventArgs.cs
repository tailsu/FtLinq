using System;
using System.Linq.Expressions;

namespace GameRayBurst.FtLinq
{
    public sealed class QueryCompiledEventArgs : EventArgs
    {
        public readonly LambdaExpression OriginalExpression;
        public readonly LambdaExpression RecompiledExpression;

        internal QueryCompiledEventArgs(LambdaExpression originalExpression, LambdaExpression recompiledExpression)
        {
            OriginalExpression = originalExpression;
            RecompiledExpression = recompiledExpression;
        }
    }
}
