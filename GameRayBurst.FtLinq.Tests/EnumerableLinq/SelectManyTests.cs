using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests.EnumerableLinq
{
    [TestClass]
    public class SelectManyTests
    {
        [TestMethod, PerformanceTest]
        public void SelectMany_ReturnsEnumerable()
        {
            FtlAssert.Works((int[] source) => source
                .SelectMany(i => Enumerable.Repeat(i, 3))
                .ToList());
        }

        [TestMethod, PerformanceTest]
        public void SelectMany_PrefixedCalls()
        {
            FtlAssert.Works((int[] source) => source
                .Where(i => i % 2 == 0)
                .SelectMany(i => Enumerable.Repeat(i, 3))
                .ToList());
        }

        [TestMethod, PerformanceTest]
        public void SelectMany_NestedWhere()
        {
            FtlAssert.Works((int[] source) => source
                .SelectMany(i => Enumerable.Repeat(i, 3))
                .Where(i => i % 2 == 0)
                .ToList());
        }

        [TestMethod, PerformanceTest]
        public void SelectMany_NestedWhereSelect()
        {
            FtlAssert.Works((int[] source) => source
                .SelectMany(i => Enumerable.Repeat(i, 3))
                .Where(i => i % 2 == 0)
                .Select(i => i * 3)
                .ToList());
        }

        [TestMethod, PerformanceTest]
        public void SelectMany_ReturnsArray()
        {
            FtlAssert.Works((int[] source) => source
                .SelectMany(i => new[] {i, i, i})
                .Where(i => i % 2 == 0)
                .Select(i => i * 3)
                .ToList());
        }

        [TestMethod]
        public void SelectMany_ReturnsEmpty()
        {
            FtlAssert.Works((int[] source) => source
                .SelectMany(i => new int[0])
                .Where(i => i % 2 == 0)
                .Select(i => i * 3)
                .ToList());
        }

        private class CountsCreations : List<int>
        {
            [ThreadStatic] public static int creations;

            public CountsCreations()
                : base(new[] {1, 2, 3})
            {
                creations++;
            }
        }

        [TestMethod]
        public void SelectMany_EnumerableCreatedOnlyOnce()
        {
            CountsCreations.creations = 0;
            var lambda = Ftl.Compile(
                (int[] source) => source
                    .SelectMany(i => new CountsCreations())
                    .ToList());

            lambda(new[] {1, 2, 3});

            Assert.AreEqual(3, CountsCreations.creations);
        }

        [TestMethod, PerformanceTest]
        public void SelectMany_ReturnsList()
        {
            FtlAssert.Works((int[] source) => source
                .SelectMany(i => new List<int> {i, i, i})
                .Where(i => i % 2 == 0)
                .Select(i => i * 3)
                .ToList());
        }

        [TestMethod, PerformanceTest]
        public void SelectMany_NestedWhereSelectAndPrefixedCalls()
        {
            FtlAssert.Works((int[] source) => source
                .Where(i => i < 90)
                .SelectMany(i => Enumerable.Repeat(i, 3))
                .Where(i => i % 2 == 0)
                .Select(i => i * 3)
                .ToList());
        }

        [TestMethod]
        public void SelectMany_BasicIndexing()
        {
            FtlAssert.Works((int[] source) =>
                source.SelectMany((n, i) => new[] {n, i}).ToList());
        }

        [TestMethod]
        public void SelectMany_BasicProjection()
        {
            FtlAssert.Works((int[] source) =>
                source.SelectMany(n => new[] {n, n * 2}, (src, coll) => src * 10 + coll).ToList());
        }

        [TestMethod]
        public void SelectMany_BasicIndexingAndProjection()
        {
            FtlAssert.Works((int[] source) =>
                source.SelectMany((n, i) => new[] { n, n * 2, i }, (src, coll) => src * 10 + coll).ToList());
        }

        [TestMethod]
        public void SelectMany_IndexingWithMultipleReferencesToIndex()
        {
            FtlAssert.Works((int[] source) =>
                source.SelectMany((n, i) => new[] { n, i, i * 3 }).ToList());
        }
    }
}
