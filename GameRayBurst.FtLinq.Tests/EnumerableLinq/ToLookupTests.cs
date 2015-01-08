using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests.EnumerableLinq
{
    [TestClass]
    public class ToLookupTests
    {
        [TestMethod, PerformanceTest]
        public void ToLookup_Minimal()
        {
            FtlAssert.LookupWorks((int[] source) => source.ToLookup(i => i % 2));
        }

        [TestMethod, PerformanceTest]
        public void ToLookup_PrefixedCalls()
        {
            FtlAssert.LookupWorks((int[] source) =>
                source
                .Where(i => i < 90)
                .SelectMany(i => new[] { i * 2, i * 3, i * 4 })
                .ToLookup(i => i % 2));
        }

        [TestMethod]
        public void ToLookup_Comparer()
        {
            FtlAssert.LookupWorks((string[] source) => source.ToLookup(s => s, StringComparer.OrdinalIgnoreCase),
                new[] { "a", "A", "aA", "Aa", "AA", "aa", "" });
        }

        [TestMethod]
        public void ToLookup_ElementSelector()
        {
            FtlAssert.LookupWorks((int[] source) => source.ToLookup(i => i % 2, i => i*2));
        }

        [TestMethod]
        public void ToLookup_ElementSelectorAndComparer()
        {
            FtlAssert.LookupWorks((string[] source) => source.ToLookup(s => s, s => s+s, StringComparer.OrdinalIgnoreCase),
                new[] { "a", "A", "aA", "Aa", "AA", "aa", "" });
        }
    }
}
