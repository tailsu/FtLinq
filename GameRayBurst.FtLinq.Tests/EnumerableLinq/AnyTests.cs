using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests.EnumerableLinq
{
    [TestClass]
    public class AnyTests
    {
        [TestMethod]
        public void Any_NonEmpty()
        {
            FtlAssert.WorksScalar((IEnumerable<int> source) => source.Any(), Enumerable.Range(1, 10));
        }

        [TestMethod]
        public void Any_Empty()
        {
            FtlAssert.WorksScalar((IEnumerable<int> source) => source.Any(), Enumerable.Range(1, 0));
        }

        [TestMethod]
        public void Any_EmptyPredicated()
        {
            FtlAssert.WorksScalar((IEnumerable<int> source) => source.Any(i => i % 5 == 0), Enumerable.Range(1, 0));
        }

        [TestMethod]
        public void Any_NonEmptyPredicated_Negative()
        {
            FtlAssert.WorksScalar((IEnumerable<int> source) => source.Any(i => i % 5 == 0), Enumerable.Range(1, 4));
        }

        [TestMethod]
        public void Any_NonEmptyPredicated_Positive()
        {
            FtlAssert.WorksScalar((IEnumerable<int> source) => source.Any(i => i % 5 == 0), Enumerable.Range(1, 10));
        }

        [TestMethod, PerformanceTest]
        public void Any_PrefixedCalls()
        {
            FtlAssert.WorksScalar((IEnumerable<int> source) =>
                source
                .Where(i => i % 2 == 0)
                .Select(i => i / 2)
                .Any(i => i % 5 == 0), Enumerable.Range(1, 10));
        }

        private class CountsTouches : IEnumerable<int>
        {
            [ThreadStatic]
            public static int touches;

            public bool Touch()
            {
                touches++;
                return true;
            }

            public IEnumerator<int> GetEnumerator()
            {
                touches++;
                yield return 1;
                touches++;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        [TestMethod]
        public void Any_Predicated_BreaksLoop()
        {
            CountsTouches.touches = 0;
            var query = Ftl.Compile((IEnumerable<CountsTouches> source) => source.Any(t => t.Touch()));
            query(Enumerable.Range(1, 10).Select(i => new CountsTouches()));
            Assert.AreEqual(1, CountsTouches.touches);
        }

        [TestMethod]
        public void Any_BreaksLoop()
        {
            CountsTouches.touches = 0;
            var query = Ftl.Compile((CountsTouches source) => source.Any());
            query(new CountsTouches());
            Assert.AreEqual(1, CountsTouches.touches);
        }
    }
}
