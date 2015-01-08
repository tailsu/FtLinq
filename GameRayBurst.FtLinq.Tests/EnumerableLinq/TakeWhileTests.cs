using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GameRayBurst.FtLinq.Tests.EnumerableLinq
{
    [TestClass]
    public class TakeWhileTests
    {
        [TestMethod, PerformanceTest]
        public void TakeWhile_Simple()
        {
            FtlAssert.Works((int[] source) =>
                source.TakeWhile(i => i < 50).ToList());
        }

        [TestMethod]
        public void TakeWhile_NoHits()
        {
            FtlAssert.Works((int[] source) =>
                source.TakeWhile(i => i < 0).ToList());
        }

        [TestMethod, PerformanceTest]
        public void TakeWhile_NestedLoop()
        {
            FtlAssert.Works((int[] source) =>
                source
                .SelectMany(i => new[] {i, i+4, i+7})
                .TakeWhile(i => i < 50).ToList());
        }
    }
}
