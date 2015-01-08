using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using GameRayBurst.FtLinq.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.PerformanceTest
{
    class Program : IPerformanceResultCollector
    {
        private static readonly object[] EmptyParams = new object[0];

        private static readonly string MethodToTest;

        static void Main(string[] args)
        {
            new Program().Run();
        }

        private void Run()
        {
            Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(1);
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            FtlAssert.TestData = Enumerable.Range(0, 100).ToArray();
            FtlAssert.Iterations = 100000;
            FtlAssert.PerformanceResultCollector = this;

            var testAttr = typeof (Tests.PerformanceTest);
            var testAssembly = testAttr.Assembly;

            var testMethods = testAssembly.GetTypes()
                .Where(t => t.GetCustomAttributes(typeof (TestClassAttribute), true).Length > 0)
                .SelectMany(t => t.GetMethods().Where(m => m.GetCustomAttributes(testAttr, true).Length > 0));

            Console.WriteLine("Test method,Reference time,FTL time,Speed-up");

            foreach (var testMethod in testMethods)
            {
                if (MethodToTest != null && testMethod.Name != MethodToTest)
                    continue;

                var classInst = Activator.CreateInstance(testMethod.DeclaringType);
                testMethod.Invoke(classInst, EmptyParams);
            }
        }

        private static Func<StackFrame[], MethodBase> GetTestMethod
            = Ftl.Compile((StackFrame[] frames) =>
                frames.Select(frame => frame.GetMethod())
                    .Where(method => method.GetCustomAttributes(typeof (Tests.PerformanceTest), true).Length > 0)
                    .First());

        void IPerformanceResultCollector.AddResult(TimeSpan referenceTime, TimeSpan ftlTime, StackTrace stackTrace)
        {
            var method = GetTestMethod(stackTrace.GetFrames());

            var ratio = (double) referenceTime.Ticks / ftlTime.Ticks;
            Console.WriteLine("{0},{1},{2},{3:#.#}x", method.Name, referenceTime.Ticks, ftlTime.Ticks, ratio);
        }
    }
}
