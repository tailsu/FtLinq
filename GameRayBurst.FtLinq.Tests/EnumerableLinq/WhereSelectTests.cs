using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests.EnumerableLinq
{
    [TestClass]
    public class WhereSelectTests
    {
        [TestMethod, PerformanceTest]
        public void Select_Simple()
        {
            FtlAssert.Works((int[] source) => source.Select(i => i * 3).ToList());
        }

        [TestMethod, PerformanceTest]
        public void Select_IgnoreValue()
        {
            FtlAssert.Works((int[] source) => source.Select(i => 17).ToList());
        }

        [TestMethod, PerformanceTest]
        public void Where_Simple()
        {
            FtlAssert.Works((int[] source) => source.Where(i => i % 2 == 0).ToList());
        }

        [TestMethod, PerformanceTest]
        public void WhereSelect_Simple()
        {
            FtlAssert.Works(
                (int[] source) =>
                source
                    .Where(i => i % 2 == 0)
                    .Select(i => i / 2)
                    .ToList());
        }

        [TestMethod, PerformanceTest]
        public void WhereSelect_Multiple()
        {
            FtlAssert.Works(
                (int[] source) =>
                source.Where(i => i % 2 == 0)
                    .Select(i => i / 2)
                    .Where(i => i % 3 == 1)
                    .Select(i => (i - 1) / 3)
                    .Select(i => i * 100)
                    .Where(i => i < 500)
                    .ToList());
        }

        [TestMethod, PerformanceTest]
        public void Select_TypeChanges()
        {
            FtlAssert.Works(
                (int[] source) =>
                    source
                        .Select(i => i * 2.0)
                        .Select(i => (byte) i)
                        .Select(i => i.ToString())
                        .ToList()
                );
        }

        [TestMethod, PerformanceTest]
        public void Select_Indexing()
        {
            FtlAssert.Works((int[] source) => source.Select((n, i) => i).ToList());
        }

        [TestMethod, PerformanceTest]
        public void Select_Indexing_BothParametersOk()
        {
            FtlAssert.Works((int[] source) => source.Select((n, i) => n * i).ToList());
        }

        [TestMethod]
        public void Select_IndexingMultipleReferencesToIndex()
        {
            FtlAssert.Works((int[] source) => source.Select((n, i) => i + i * n).ToList());
        }

        [TestMethod, PerformanceTest]
        public void Select_IndexingWithPrefixedFiltering_IndexingCountsTheRightSequence()
        {
            FtlAssert.Works((int[] source) =>
                source
                .Where(i => i % 3 == 0)
                .Select((n, i) => i).ToList());
        }

        [TestMethod, PerformanceTest]
        public void Select_MultipleIndexingSelects_IndicesDontGetThrashed()
        {
            FtlAssert.Works((int[] source) =>
                source
                .Select((n, i) => i + 10)
                .Where(i => i > 20)
                .Select((n, i) => i + 30)
                .ToList());
        }

        [TestMethod]
        public void Select_NonRewritableDelegate()
        {
            Func<int, int> f = i => i * 2;
            FtlAssert.Works((int[] source) => source.Select(f).ToList());
        }

        [TestMethod]
        public void Select_IndexingNonRewritableDelegate()
        {
            Func<int, int, int> f = (i, x) => i * 2 + x;
            FtlAssert.Works((int[] source) => source.Select(f).ToList());
        }

        [TestMethod, PerformanceTest]
        public void Where_Indexing()
        {
            FtlAssert.Works((int[] source) => source.Where((n, i) => i % 5 != 4).ToList());
        }

        [TestMethod, PerformanceTest]
        public void Where_Indexing_BothParametersOk()
        {
            FtlAssert.Works((int[] source) => source.Where((n, i) => (n*i) % 5 != 4).ToList());
        }

        [TestMethod, PerformanceTest]
        public void Where_MultipleIndexingWheres_IndicesDontGetThrashed()
        {
            FtlAssert.Works((int[] source) =>
                source
                .Where((n, i) => i < 90)
                .Select((n, i) => i + 30)
                .Where((n, i) => i % 5 != 4)
                .ToList());
        }

        [TestMethod, PerformanceTest]
        public void Where_IndexingAndSelectMany()
        {
            FtlAssert.Works((int[] source) =>
                source.Where((n, i) => i > 90)
                .SelectMany(n => new[] {n, n*2})
                .ToList());
        }

        [TestMethod]
        public void Where_NonRewritableDelegate()
        {
            Func<int, bool> f = i => i > 50;
            FtlAssert.Works((int[] source) => source.Where(f).ToList());
        }

        [TestMethod]
        public void Where_IndexingNonRewritableDelegate()
        {
            Func<int, int, bool> f = (i, x) => i + x < 50;
            FtlAssert.Works((int[] source) => source.Where(f).ToList());
        }

        [TestMethod]
        public void Where_IndexingMultipleReferencesToIndex()
        {
            FtlAssert.Works((int[] source) => source.Where((n, i) => i + i*n < 50).ToList());
        }
    }
}
