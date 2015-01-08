using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests.EnumerableLinq
{
    [TestClass]
    public class OrderByTests
    {
        [TestMethod, PerformanceTest]
        public void OrderBy_IdentityKey()
        {
            FtlAssert.Works((int[] source) => source.OrderBy(_ => _).ToList(), FtlAssert.RandomData);
        }

        [TestMethod, PerformanceTest]
        public void OrderBy_WithKeyFunction()
        {
            FtlAssert.Works((int[] source) => source.OrderBy(i => -i).ToList(), FtlAssert.RandomData);
        }

        [TestMethod, PerformanceTest]
        public void OrderBy_ThenBy_ToList()
        {
            FtlAssert.Works((int[] source) => source.OrderBy(i => i % 10).ThenBy(i => i / 10).ToList(), FtlAssert.RandomData);
        }

        [TestMethod, PerformanceTest]
        public void OrderBy_ThenBy_ToArray()
        {
            FtlAssert.Works((int[] source) => source.OrderBy(i => i % 10).ThenBy(i => i / 10).ToArray(), FtlAssert.RandomData);
        }
    }
}
