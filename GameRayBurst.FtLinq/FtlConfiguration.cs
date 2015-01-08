using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler;
using GameRayBurst.FtLinq.Recompiler.Enumerable;
using GameRayBurst.FtLinq.Recompiler.FtlEnumerable;
using GameRayBurst.FtLinq.Recompiler.Iterators;
using Single = GameRayBurst.FtLinq.Recompiler.Enumerable.Single;

namespace GameRayBurst.FtLinq
{
    public sealed class DiagnosticEventArgs : EventArgs
    {
        public readonly string Message;
        public readonly Expression Origin;

        public DiagnosticEventArgs(string message, Expression origin)
        {
            Message = message;
            Origin = origin;
        }
    }

    public static class FtlConfiguration
    {
        public static readonly MethodRegistry<IIterationMethod, Type> IterationMethods = new MethodRegistry<IIterationMethod, Type>();
        public static readonly MethodRegistry<IAggregationMethod, MethodInfo> AggregationMethods = new MethodRegistry<IAggregationMethod, MethodInfo>();
        public static readonly MethodRegistry<ITransformMethod, MethodInfo> TransformMethods = new MethodRegistry<ITransformMethod, MethodInfo>();
        private static bool thePrintDiagnosticInfo;

        public static event EventHandler<DiagnosticEventArgs> Diagnostic;

        public static bool PrintDiagnosticInfo
        {
            get { return thePrintDiagnosticInfo; }
            set
            {
                if (thePrintDiagnosticInfo != value)
                {
                    thePrintDiagnosticInfo = value;
                    if (value)
                        Diagnostic += PrintDiagnosticToDebug;
                    else
                        Diagnostic -= PrintDiagnosticToDebug;
                }
            }
        }

        internal static void EmitDiagnostic(string message, Expression origin)
        {
            var handler = Diagnostic;
            if (handler != null)
            {
                handler(null, new DiagnosticEventArgs(message, origin));
            }
        }

        private static void PrintDiagnosticToDebug(object sender, DiagnosticEventArgs e)
        {
            Debug.WriteLine(e.Message);
        }

        static FtlConfiguration()
        {
            TransformMethods.Register(new MethodOptionTransformMethod());

            RegisterLinqEnumerableMethods();
            RegisterFtlEnumerableMethods();
        }

        private static void RegisterLinqEnumerableMethods()
        {
            IterationMethods.Register(new EnumerableIterationMethod());
            IterationMethods.Register(new ListIterationMethod());
            IterationMethods.Register(new ArrayIterationMethod());

            AggregationMethods.Register(new ToArray());
            AggregationMethods.Register(new ToList());
            AggregationMethods.Register(new ToDictionary());
            AggregationMethods.Register(new GroupBy());
            AggregationMethods.Register(new ToLookup());
            AggregationMethods.Register(new Sum());
            AggregationMethods.Register(new Count());
            AggregationMethods.Register(new LongCount());
            AggregationMethods.Register(new Min());
            AggregationMethods.Register(new Max());
            AggregationMethods.Register(new Contains());
            AggregationMethods.Register(new Any());
            AggregationMethods.Register(new All());
            AggregationMethods.Register(new Aggregate());
            AggregationMethods.Register(new Average());
            AggregationMethods.Register(new First());
            AggregationMethods.Register(new FirstOrDefault());
            AggregationMethods.Register(new Last());
            AggregationMethods.Register(new LastOrDefault());
            AggregationMethods.Register(new Single());
            AggregationMethods.Register(new SingleOrDefault());
            AggregationMethods.Register(new ElementAt());
            AggregationMethods.Register(new ElementAtOrDefault());

            TransformMethods.Register(new Select());
            TransformMethods.Register(new SelectMany());
            TransformMethods.Register(new Where());
            TransformMethods.Register(new Cast());
            TransformMethods.Register(new OfType());
            TransformMethods.Register(new TakeWhile());
            TransformMethods.Register(new Take());
            TransformMethods.Register(new SkipWhile());
            TransformMethods.Register(new Skip());
            TransformMethods.Register(new Distinct());
            TransformMethods.Register(new OrderBy());
        }

        private static void RegisterFtlEnumerableMethods()
        {
            IterationMethods.Register(new RepetitionIterationMethod());
            IterationMethods.Register(new RangeIterationMethod());
            IterationMethods.Register(new GeneratorIterationMethod());

            AggregationMethods.Register(new ToSet());
        }
    }
}
