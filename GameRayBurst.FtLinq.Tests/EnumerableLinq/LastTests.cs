using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests.EnumerableLinq
{
    [TestClass]
    public class LastTests
    {
        [TestMethod]
        public void Last_Basic()
        {
            FtlAssert.WorksScalar((int[] source) => source.Last(), FtlAssert.TestData);
        }

        [TestMethod]
        public void Last_Empty()
        {
            FtlAssert.Throws((int[] source) => source.Last(), new int[0]);
        }

        [TestMethod]
        public void Last_WithFilter()
        {
            FtlAssert.WorksScalar((int[] source) => source.Where(i => i < 50).Last());
        }

        [TestMethod, PerformanceTest]
        public void Last_Predicated()
        {
            FtlAssert.WorksScalar((int[] source) => source.Last(i => i > 50), FtlAssert.TestData);
        }

        [TestMethod]
        public void Last_PredicatedEmpty()
        {
            FtlAssert.Throws((int[] source) => source.Last(i => i > 50), new int[0]);
        }

        [TestMethod]
        public void Last_PredicatedNoHits()
        {
            FtlAssert.Throws((int[] source) => source.Last(i => i > 50), Enumerable.Range(1, 10).ToArray());
        }

        [TestMethod]
        public void Last_BreakFromNestedLoop()
        {
            FtlAssert.WorksScalar((int[] source) =>
                source
                .SelectMany(i => new[] { i, i, i })
                .Last(), FtlAssert.TestData);
        }

        [TestMethod]
        public void LastOrDefault_BreakFromNestedLoop()
        {
            FtlAssert.WorksScalar((int[] source) =>
                source
                .SelectMany(i => new[] { i, i, i })
                .LastOrDefault(), FtlAssert.TestData);
        }

        [TestMethod]
        public void LastOrDefault_Basic()
        {
            FtlAssert.WorksScalar((int[] source) => source.LastOrDefault(), FtlAssert.TestData);
        }

        [TestMethod]
        public void LastOrDefault_Empty()
        {
            FtlAssert.WorksScalar((int[] source) => source.LastOrDefault(), new int[0]);
        }

        [TestMethod, PerformanceTest]
        public void LastOrDefault_Predicated()
        {
            FtlAssert.WorksScalar((int[] source) => source.LastOrDefault(i => i > 50), FtlAssert.TestData);
        }

        [TestMethod]
        public void LastOrDefault_PredicatedEmpty()
        {
            FtlAssert.WorksScalar((int[] source) => source.LastOrDefault(i => i > 50), new int[0]);
        }

        [TestMethod]
        public void LastOrDefault_PredicatedNoHits()
        {
            FtlAssert.WorksScalar((int[] source) => source.LastOrDefault(i => i > 50),
                Enumerable.Range(1, 30).ToArray());
        }

        [TestMethod]
        public void LastOrDefault_ReferenceTypeNoHits()
        {
            FtlAssert.WorksScalar((object[] source) => source.LastOrDefault(i => (int)i > 50),
                Enumerable.Range(1, 30).Select(i => (object)i).ToArray());
        }

        [TestMethod]
        public void LastOrDefault_WithFilter()
        {
            FtlAssert.WorksScalar((int[] source) => source.Where(i => i < 50).LastOrDefault());
        }
    }
}
