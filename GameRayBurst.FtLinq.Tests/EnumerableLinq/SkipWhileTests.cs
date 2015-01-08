using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GameRayBurst.FtLinq.Tests.EnumerableLinq
{
    [TestClass]
    public class SkipWhileTests
    {
        [TestMethod, PerformanceTest]
        public void SkipWhile_Simple()
        {
            FtlAssert.Works((int[] source) =>
                source.SkipWhile(i => i > 50).ToList());
        }

        [TestMethod, PerformanceTest]
        public void SkipWhile_NoSkips()
        {
            FtlAssert.Works((int[] source) =>
                source.SkipWhile(i => i >= 0).ToList());
        }

        [TestMethod, PerformanceTest]
        public void SkipWhile_NestedLoopInArray()
        {
            FtlAssert.Works((int[] source) =>
                source
                .SelectMany(i => new[] { i, i+1, i+2 })
                .SkipWhile(i => i < 10).ToList());
        }

        [TestMethod, PerformanceTest]
        public void SkipWhile_NestedLoopInList()
        {
            FtlAssert.Works((int[] source) =>
                source
                .SelectMany(i => new List<int> { i, i+1, i+2 })
                .SkipWhile(i => i < 10).ToList());
        }

        [TestMethod, PerformanceTest]
        public void SkipWhile_NestedLoopInEnumerable()
        {
            FtlAssert.Works((int[] source) =>
                source
                .SelectMany(i => new HashSet<int> { i, i+1, i+2 })
                .SkipWhile(i => i < 10).ToList());
        }
    }
}
