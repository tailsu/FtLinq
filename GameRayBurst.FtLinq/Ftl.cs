using System;
using System.Linq.Expressions;
using GameRayBurst.FtLinq.Recompiler;

namespace GameRayBurst.FtLinq
{
    public static class Ftl
    {
        public static EventHandler<QueryCompiledEventArgs> QueryCompiled;

        public static Delegate DynamicCompile(Expression query)
        {
            if (query.NodeType != ExpressionType.Lambda)
                throw new FtlException(query, "Expression not a lambda");

            var lambda = (LambdaExpression) query;
            var recompiler = new RecompilationState(lambda);
            var queryFunc = recompiler.BuildImperativeVersion();

            var handler = QueryCompiled;
            if (handler != null)
                handler(null, new QueryCompiledEventArgs(lambda, queryFunc));

            return queryFunc.Compile();
        }

        public static Func<TAggregatedResult>
            Compile<TAggregatedResult>
            (Expression<Func<TAggregatedResult>> query)
        {
            return (Func<TAggregatedResult>) DynamicCompile(query);
        }

        public static Func<TSource, TAggregatedResult>
            Compile<TSource, TAggregatedResult>
            (Expression<Func<TSource, TAggregatedResult>> query)
        {
            return (Func<TSource, TAggregatedResult>) DynamicCompile(query);
        }

        public static Func<TSource, T0, TAggregatedResult>
            Compile<TSource, T0, TAggregatedResult>
            (Expression<Func<TSource, T0, TAggregatedResult>> query)
        {
            return (Func<TSource, T0, TAggregatedResult>) DynamicCompile(query);
        }

        public static Func<TSource, T0, T1, TAggregatedResult>
            Compile<TSource, T0, T1, TAggregatedResult>
            (Expression<Func<TSource, T0, T1, TAggregatedResult>> query)
        {
            return (Func<TSource, T0, T1, TAggregatedResult>) DynamicCompile(query);
        }

        public static Func<TSource, T0, T1, T2, TAggregatedResult>
            Compile<TSource, T0, T1, T2, TAggregatedResult>
            (Expression<Func<TSource, T0, T1, T2, TAggregatedResult>> query)
        {
            return (Func<TSource, T0, T1, T2, TAggregatedResult>) DynamicCompile(query);
        }

        public static Func<TSource, T0, T1, T2, T3, TAggregatedResult>
            Compile<TSource, T0, T1, T2, T3, TAggregatedResult>
            (Expression<Func<TSource, T0, T1, T2, T3, TAggregatedResult>> query)
        {
            return (Func<TSource, T0, T1, T2, T3, TAggregatedResult>) DynamicCompile(query);
        }

        public static Func<TSource, T0, T1, T2, T3, T4, TAggregatedResult>
            Compile<TSource, T0, T1, T2, T3, T4, TAggregatedResult>
            (Expression<Func<TSource, T0, T1, T2, T3, T4, TAggregatedResult>> query)
        {
            return (Func<TSource, T0, T1, T2, T3, T4, TAggregatedResult>) DynamicCompile(query);
        }

        public static Func<TSource, T0, T1, T2, T3, T4, T5, TAggregatedResult>
            Compile<TSource, T0, T1, T2, T3, T4, T5, TAggregatedResult>
            (Expression<Func<TSource, T0, T1, T2, T3, T4, T5, TAggregatedResult>> query)
        {
            return (Func<TSource, T0, T1, T2, T3, T4, T5, TAggregatedResult>) DynamicCompile(query);
        }

        public static Func<TSource, T0, T1, T2, T3, T4, T5, T6, TAggregatedResult>
            Compile<TSource, T0, T1, T2, T3, T4, T5, T6, TAggregatedResult>
            (Expression<Func<TSource, T0, T1, T2, T3, T4, T5, T6, TAggregatedResult>> query)
        {
            return (Func<TSource, T0, T1, T2, T3, T4, T5, T6, TAggregatedResult>) DynamicCompile(query);
        }

        public static Func<TSource, T0, T1, T2, T3, T4, T5, T6, T7, TAggregatedResult>
            Compile<TSource, T0, T1, T2, T3, T4, T5, T6, T7, TAggregatedResult>
            (Expression<Func<TSource, T0, T1, T2, T3, T4, T5, T6, T7, TAggregatedResult>> query)
        {
            return (Func<TSource, T0, T1, T2, T3, T4, T5, T6, T7, TAggregatedResult>) DynamicCompile(query);
        }

        public static Func<TSource, T0, T1, T2, T3, T4, T5, T6, T7, T8, TAggregatedResult>
            Compile<TSource, T0, T1, T2, T3, T4, T5, T6, T7, T8, TAggregatedResult>
            (Expression<Func<TSource, T0, T1, T2, T3, T4, T5, T6, T7, T8, TAggregatedResult>> query)
        {
            return (Func<TSource, T0, T1, T2, T3, T4, T5, T6, T7, T8, TAggregatedResult>) DynamicCompile(query);
        }

        public static Func<TSource, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TAggregatedResult>
            Compile<TSource, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TAggregatedResult>
            (Expression<Func<TSource, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TAggregatedResult>> query)
        {
            return (Func<TSource, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TAggregatedResult>) DynamicCompile(query);
        }
    }
}
