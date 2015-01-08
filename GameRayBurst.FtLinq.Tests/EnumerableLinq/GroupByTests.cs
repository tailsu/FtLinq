using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests.EnumerableLinq
{
    [TestClass]
    public class GroupByTests
    {
        [TestMethod, PerformanceTest]
        public void GroupBy_Minimal()
        {
            FtlAssert.GroupingWorks((int[] source) => source.GroupBy(i => i % 2));
        }

        [TestMethod, PerformanceTest]
        public void GroupBy_PrefixedCalls()
        {
            FtlAssert.GroupingWorks((int[] source) =>
                source
                .Where(i => i < 90)
                .SelectMany(i => new[] { i * 2, i * 3, i * 4})
                .GroupBy(i => i % 2));
        }

        [TestMethod]
        public void GroupBy_Comparer()
        {
            FtlAssert.GroupingWorks((string[] source) => source.GroupBy(s => s, StringComparer.OrdinalIgnoreCase),
                new[] {"a", "A", "aA", "Aa", "AA", "aa", ""} );
        }

        [TestMethod]
        public void GroupBy_ElementSelector()
        {
            FtlAssert.GroupingWorks((int[] source) => source.GroupBy(i => i % 2, i => i*2));
        }

        [TestMethod]
        [Ignore]
        public void GroupBy_ResultSelector()
        {
            FtlAssert.Works((int[] source) => source.GroupBy(i => i % 2, (i, g) => g.Sum()));
        }

        [TestMethod]
        [Ignore]
        public void GroupBy_ElementSelectorAndResultSelector()
        {
            FtlAssert.Works((int[] source) => source.GroupBy(i => i % 2, i => i * 2, (i, g) => g.Sum()));
        }

        [TestMethod]
        public void GroupBy_ElementSelectorAndComparer()
        {
            FtlAssert.GroupingWorks((string[] source) => source.GroupBy(s => s, s => s+s, StringComparer.OrdinalIgnoreCase),
                new[] { "a", "A", "aA", "Aa", "AA", "aa", "" });
        }

        [TestMethod]
        [Ignore]
        public void GroupBy_ResultSelectorAndComparer()
        {
            FtlAssert.Works((string[] source) => source.GroupBy(s => s, (i, g) => g.First() + g.Last(), StringComparer.OrdinalIgnoreCase),
                new[] { "a", "A", "aA", "Aa", "AA", "aa", "" });
        }

        [TestMethod]
        [Ignore]
        public void GroupBy_ElementSelectorAndResultSelectorAndComparer()
        {
            FtlAssert.Works((string[] source) => source.GroupBy(s => s, s => s+s, (i, g) => g.First() + g.Last(), StringComparer.OrdinalIgnoreCase),
                new[] { "a", "A", "aA", "Aa", "AA", "aa", "" });
        }
    }
}
