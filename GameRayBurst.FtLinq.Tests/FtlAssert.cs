using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class PerformanceTest : Attribute {}

    public class FtlAssert
    {
        public static void Works<TOutput>(Expression<Func<int[], TOutput>> expr)
            where TOutput : IEnumerable
        {
            Works(expr, TestData);
        }

        public static void Works<TInput, TOutput>(
            Expression<Func<TInput, TOutput>> expr,
            TInput testData)
            where TOutput : IEnumerable
        {
            var reference = expr.Compile();

            var query = (Func<TInput, TOutput>) Ftl.Compile(expr);

            if (PerformanceResultCollector == null)
            {
                var referenceData = reference(testData);
                var ftlData = query(testData);
                Assert.IsTrue(referenceData.Cast<object>().SequenceEqual(ftlData.Cast<object>()));
            }
            else
            {
                long tStart, tEnd;

                reference(testData); // warm up
                GetSystemTimeAsFileTime(out tStart);
                for (int i = 0; i < Iterations; ++i)
                    reference(testData);
                GetSystemTimeAsFileTime(out tEnd);
                var referenceTime = TimeSpan.FromTicks(tEnd - tStart);

                query(testData); // warm up
                GetSystemTimeAsFileTime(out tStart);
                for (int i = 0; i < Iterations; ++i)
                    query(testData);
                GetSystemTimeAsFileTime(out tEnd);
                var ftlTime = TimeSpan.FromTicks(tEnd - tStart);

                PerformanceResultCollector.AddResult(referenceTime, ftlTime, new StackTrace());
            }
        }

        public static void Works<TInput, TParam1, TOutput>(
            Expression<Func<TInput, TParam1, TOutput>> expr,
            TParam1 param1,
            TInput testData)
            where TOutput : ICollection
        {
            var reference = expr.Compile();

            var query = (Func<TInput, TParam1, TOutput>) Ftl.DynamicCompile(expr);

            var referenceData = reference(testData, param1);
            var ftlData = query(testData, param1);

            CollectionAssert.AreEqual(referenceData, ftlData);
        }

        public static void Works<TInput, TParam1, TParam2, TOutput>(
            Expression<Func<TInput, TParam1, TParam2, TOutput>> expr,
            TParam1 param1, TParam2 param2,
            TInput testData)
            where TOutput : ICollection
        {
            var reference = expr.Compile();

            var query = (Func<TInput, TParam1, TParam2, TOutput>) Ftl.DynamicCompile(expr);

            var referenceData = reference(testData, param1, param2);
            var ftlData = query(testData, param1, param2);

            CollectionAssert.AreEqual(referenceData, ftlData);
        }

        public static void WorksScalar<TOutput>(Expression<Func<int[], TOutput>> expr)
        {
            WorksScalar(expr, TestData);
        }

        public static void WorksScalar<TInput, TOutput>(Expression<Func<TInput, TOutput>> expr, TInput testData)
        {
            var reference = expr.Compile();

            var query = (Func<TInput, TOutput>) Ftl.DynamicCompile(expr);

            var referenceData = reference(testData);
            var ftlData = query(testData);

            Assert.AreEqual(referenceData, ftlData);
        }

        public static void WorksScalar<TOutput>(Expression<Func<TOutput>> expr)
        {
            var reference = expr.Compile();

            var query = (Func<TOutput>) Ftl.DynamicCompile(expr);

            var referenceData = reference();
            var ftlData = query();

            Assert.AreEqual(referenceData, ftlData);
        }

        public static void GroupingWorks<TKey, TValue>(Expression<Func<int[], IEnumerable<IGrouping<TKey, TValue>>>> expr)
        {
            GroupingWorks(expr, FtlAssert.TestData);
        }

        public static void GroupingWorks<TInput, TKey, TValue>(Expression<Func<TInput, IEnumerable<IGrouping<TKey, TValue>>>> expr, TInput testData)
        {
            var reference = expr.Compile();

            var query = (Func<TInput, IEnumerable<IGrouping<TKey, TValue>>>) Ftl.DynamicCompile(expr);

            if (PerformanceResultCollector == null)
            {
                var referenceData = reference(testData).OrderBy(g => g.Key);
                var ftlData = query(testData).OrderBy(g => g.Key);

                Assert.AreEqual(referenceData.Count(), ftlData.Count());

                foreach (var pair in referenceData.Zip(ftlData, (r, ftl) => new {r, ftl}))
                {
                    Assert.AreEqual(pair.r.Key, pair.ftl.Key);

                    CollectionAssert.AreEquivalent(pair.r.ToArray(), pair.ftl.ToArray());
                }
            }
            else
            {
                long tStart, tEnd;

                reference(testData); // warm up
                GetSystemTimeAsFileTime(out tStart);
                for (int i = 0; i < Iterations; ++i)
                    reference(testData);
                GetSystemTimeAsFileTime(out tEnd);
                var referenceTime = TimeSpan.FromTicks(tEnd - tStart);

                query(testData); // warm up
                GetSystemTimeAsFileTime(out tStart);
                for (int i = 0; i < Iterations; ++i)
                    query(testData);
                GetSystemTimeAsFileTime(out tEnd);
                var ftlTime = TimeSpan.FromTicks(tEnd - tStart);

                PerformanceResultCollector.AddResult(referenceTime, ftlTime, new StackTrace());
            }
        }

        public static void LookupWorks<TKey, TValue>(Expression<Func<int[], ILookup<TKey, TValue>>> expr)
        {
            LookupWorks(expr, TestData);
        }

        public static void LookupWorks<TInput, TKey, TValue>(Expression<Func<TInput, ILookup<TKey, TValue>>> expr, TInput testData)
        {
            var reference = expr.Compile();

            var query = (Func<TInput, ILookup<TKey, TValue>>) Ftl.DynamicCompile(expr);

            var referenceData = reference(testData).OrderBy(g => g.Key);
            var ftlData = query(testData).OrderBy(g => g.Key);

            Assert.AreEqual(referenceData.Count(), ftlData.Count());

            foreach (var pair in referenceData.Zip(ftlData, (r, ftl) => new { r, ftl }))
            {
                Assert.AreEqual(pair.r.Key, pair.ftl.Key);

                CollectionAssert.AreEquivalent(pair.r.ToArray(), pair.ftl.ToArray());
            }
        }

        public static void Throws<TInput, TOutput>(Expression<Func<TInput, TOutput>> expr, TInput input)
        {
            var reference = expr.Compile();
            Exception expectedException = null;
            try
            {
                reference(input);
            }
            catch (Exception ex)
            {
                expectedException = ex;
            }

            var query = (Func<TInput, TOutput>) Ftl.DynamicCompile(expr);
            Exception actualException = null;
            try
            {
                query(input);
            }
            catch (Exception ex)
            {
                actualException = ex;
            }

            Assert.IsNotNull(expectedException);
            Assert.IsNotNull(actualException);
            Assert.AreSame(expectedException.GetType(), actualException.GetType());
            Assert.AreEqual(expectedException.Message, actualException.Message);
        }

        public static void Throws<TException>(Action action) where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
                return;
            }
            catch (Exception ex)
            {
                Assert.Fail("Wrong exception thrown: '{0}'", ex.GetType());
            }
            Assert.Fail("No exception thrown. Expected: '{0}'", typeof(TException));
        }

        /////////////////////////////////////////////////////////////////////////
        public static IPerformanceResultCollector PerformanceResultCollector;

        private static readonly Random RNG = new Random(443355);
        public static int[] TestData = Enumerable.Range(1, 100).ToArray();
        public static int[] RandomData = FtlEnumerable.Generate(() => RNG.Next(100), 100).ToArray();

        public static int Iterations = 1;

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern void GetSystemTimeAsFileTime(out long time);
    }

    public interface IPerformanceResultCollector
    {
        void AddResult(TimeSpan referenceTime, TimeSpan ftlTime, StackTrace stackTrace);
    }
}
