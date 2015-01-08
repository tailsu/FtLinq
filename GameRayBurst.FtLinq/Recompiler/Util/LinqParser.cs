using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;

namespace GameRayBurst.FtLinq.Recompiler.Util
{
    public sealed class Call
    {
        public readonly Expression OriginalExpression;
        public ReadOnlyCollection<MethodCallExpression> Options { get; private set; }

        public MethodCallExpression MethodCallExpression
        {
            get { return OriginalExpression as MethodCallExpression; }
        }

        public MethodInfo Method
        {
            get { return MethodCallExpression != null ? MethodCallExpression.Method : null; }
        }

        internal Call(Expression originalExpr)
        {
            OriginalExpression = originalExpr;
            Options = NoOptions;
        }

        internal void SetOptions(IEnumerable<MethodCallExpression> optionMethods)
        {
            var optionMethodList = optionMethods.ToList();
            if (optionMethodList.Count == 0)
                return;

            var method = MethodCallExpression.Method;
            foreach (var optionMethod in optionMethodList)
            {
                var compatibilityAttributes = LinqParser.GetCompatibilityAttributes(optionMethod);
                bool isCompatible = compatibilityAttributes.Any(attr =>
                    attr.IsGlobal
                    || ((attr.ModifiedMethodClass == null || attr.ModifiedMethodClass == method.DeclaringType)
                        && (attr.ModifiedMethodName == null || attr.ModifiedMethodName == method.Name)));
                if (!isCompatible)
                {
                    throw new FtlException(OriginalExpression, String.Format("Option '{0}' is not compatible with method '{1}'", optionMethod, OriginalExpression));
                }
            }

            Options = optionMethodList.AsReadOnly();
        }

        private static readonly ReadOnlyCollection<MethodCallExpression> NoOptions
            = new List<MethodCallExpression>().AsReadOnly();
    }

    internal static class LinqParser
    {
        public static List<Call> ParseLinqExpression(Expression linqExpression)
        {
            var calls = new List<Call>();
            ParseCallList(linqExpression, calls);
            ConfigureCallOptions(calls);
            return calls;
        }

        private static void ParseCallList(Expression linqExpression, List<Call> calls)
        {
            var expr = linqExpression;
            bool outermostCall = true;
            while (expr != null && (outermostCall || ReturnsEnumerable(expr)))
            {
                outermostCall = false;
                calls.Add(new Call(expr));

                var methodCall = expr as MethodCallExpression;
                expr = methodCall != null && methodCall.Arguments.Count > 0 ? methodCall.Arguments[0] : null;
            }
        }

        private static void ConfigureCallOptions(List<Call> calls)
        {
            var globalOptions = new List<MethodCallExpression>();
            var localOptions = new List<MethodCallExpression>();
            for (int i = calls.Count - 1; i >= 0; --i)
            {
                var call = calls[i];
                var methodCall = call.MethodCallExpression;
                if (methodCall == null)
                    continue;

                var compatibilityAttributes = GetCompatibilityAttributes(methodCall);
                var anyLocals = compatibilityAttributes.Any(attr => !attr.IsGlobal);
                var anyGlobals = compatibilityAttributes.Any(attr => attr.IsGlobal);
                if (anyGlobals && anyLocals)
                    throw new FtlException(methodCall, "An option may either be global or local, but not both.");

                if (anyLocals)
                    localOptions.Add(methodCall);
                else if (anyGlobals)
                {
                    var areNonspecific = compatibilityAttributes.All(attr =>
                        attr.ModifiedMethodClass == null && attr.ModifiedMethodName == null);
                    if (!areNonspecific)
                        throw new FtlException(methodCall, "Global options must not specify a modified method. Method: {0}");
                    globalOptions.Add(methodCall);
                }
                if (compatibilityAttributes.Count > 0)
                    continue;

                call.SetOptions(globalOptions.Concat(localOptions));
                localOptions.Clear();
            }
        }

        private static bool ReturnsEnumerable(Expression call)
        {
            var retType = call.Type;
            return retType.GetImplementedEnumerableInterface() != null;
        }

        internal static List<FtlMethodOptionAttribute> GetCompatibilityAttributes(MethodCallExpression expr)
        {
            return expr.Method.GetCustomAttributes(typeof(FtlMethodOptionAttribute), false).Cast<FtlMethodOptionAttribute>().ToList();   
        }
    }
}
