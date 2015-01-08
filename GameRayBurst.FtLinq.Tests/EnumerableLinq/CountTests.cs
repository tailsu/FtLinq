using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests.EnumerableLinq
{
    [TestClass]
    public class CountTests
    {
        [TestMethod, PerformanceTest]
        public void Count_Simple()
        {
            FtlAssert.WorksScalar((int[] source) => source.Count(), FtlAssert.TestData);
        }

        [TestMethod, PerformanceTest]
        public void LongCount_Simple()
        {
            FtlAssert.WorksScalar((int[] source) => source.LongCount(), FtlAssert.TestData);
        }

        [TestMethod]
        public void Count_Empty()
        {
            FtlAssert.WorksScalar((int[] source) => source.Count(), new int[0]);
        }

        [TestMethod]
        public void LongCount_Empty()
        {
            FtlAssert.WorksScalar((int[] source) => source.LongCount(), new int[0]);
        }

        [TestMethod, PerformanceTest]
        public void Count_Predicated()
        {
            FtlAssert.WorksScalar((int[] source) => source.Count(i => i % 3 == 0), FtlAssert.TestData);
        }

        [TestMethod, PerformanceTest]
        public void LongCount_Predicated()
        {
            FtlAssert.WorksScalar((int[] source) => source.LongCount(i => i % 3 == 0), FtlAssert.TestData);
        }

        [TestMethod]
        public void Count_PredicatedEmpty()
        {
            FtlAssert.WorksScalar((int[] source) => source.Count(i => i % 3 == 0), new int[0]);
        }

        [TestMethod]
        public void LongCount_PredicatedEmpty()
        {
            FtlAssert.WorksScalar((int[] source) => source.LongCount(i => i % 3 == 0), new int[0]);
        }

        [TestMethod, PerformanceTest]
        public void Count_PrefixedFilter()
        {
            FtlAssert.WorksScalar((int[] source) => source.Where(i => i % 3 == 0).Count(), FtlAssert.TestData);
        }

        [TestMethod, PerformanceTest]
        public void LongCount_PrefixedFilter()
        {
            FtlAssert.WorksScalar((int[] source) => source.Where(i => i % 3 == 0).LongCount(), FtlAssert.TestData);
        }
    }
}
