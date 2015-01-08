using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests.EnumerableLinq
{
    [TestClass]
    public class SumTests
    {
        [TestMethod, PerformanceTest]
        public void Sum_Doubles()
        {
            FtlAssert.WorksScalar(
                (double[] source) => source.Sum(),
                FtlAssert.TestData.Select(i => (double) i).ToArray());
        }

        [TestMethod]
        public void Sum_Empty()
        {
            FtlAssert.WorksScalar(
                (double[] source) => source.Sum(),
                Enumerable.Repeat(100, 0).Select(i => (double) i).ToArray());
        }

        [TestMethod, PerformanceTest]
        public void Sum_DoublesProjection()
        {
            FtlAssert.WorksScalar(
                (int[] source) => source.Sum(i => i * 2.0),
                FtlAssert.TestData);
        }

        [TestMethod]
        public void Sum_DoublesEmptyNullable()
        {
            FtlAssert.WorksScalar(
                (double?[] source) => source.Sum(),
                Enumerable.Range(1, 0).Select(i => (double?) i).ToArray());
        }

        [TestMethod, PerformanceTest]
        public void Sum_DoublesNullable()
        {
            FtlAssert.WorksScalar(
                (double?[] source) => source.Sum(),
                FtlAssert.TestData.Select(i => (double?) i).ToArray());
        }
    }
}
