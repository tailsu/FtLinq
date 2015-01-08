using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GameRayBurst.FtLinq.Tests.EnumerableLinq
{
    [TestClass]
    public class FirstTests
    {
        [TestMethod]
        public void First_Basic()
        {
            FtlAssert.WorksScalar((int[] source) => source.First(), FtlAssert.TestData);
        }

        [TestMethod]
        public void First_Empty()
        {
            FtlAssert.Throws((int[] source) => source.First(), new int[0]);
        }

        [TestMethod, PerformanceTest]
        public void First_Predicated()
        {
            FtlAssert.WorksScalar((int[] source) => source.First(i => i > 50), FtlAssert.TestData);
        }

        [TestMethod]
        public void First_PredicatedEmpty()
        {
            FtlAssert.Throws((int[] source) => source.First(i => i > 50), new int[0]);
        }

        [TestMethod]
        public void First_PredicatedNoHits()
        {
            FtlAssert.Throws((int[] source) => source.First(i => i > 50), Enumerable.Range(1, 10).ToArray());
        }

        [TestMethod]
        public void First_BreakFromNestedLoop()
        {
            FtlAssert.WorksScalar((int[] source) =>
                source
                .SelectMany(i => new[] { i, i, i })
                .First(), FtlAssert.TestData);
        }

        [TestMethod]
        public void FirstOrDefault_BreakFromNestedLoop()
        {
            FtlAssert.WorksScalar((int[] source) =>
                source
                .SelectMany(i => new[] { i, i, i })
                .FirstOrDefault(), FtlAssert.TestData);
        }

        [TestMethod]
        public void FirstOrDefault_Basic()
        {
            FtlAssert.WorksScalar((int[] source) => source.FirstOrDefault(), FtlAssert.TestData);
        }

        [TestMethod]
        public void FirstOrDefault_Empty()
        {
            FtlAssert.WorksScalar((int[] source) => source.FirstOrDefault(), new int[0]);
        }

        [TestMethod, PerformanceTest]
        public void FirstOrDefault_Predicated()
        {
            FtlAssert.WorksScalar((int[] source) => source.FirstOrDefault(i => i > 50), FtlAssert.TestData);
        }

        [TestMethod]
        public void FirstOrDefault_PredicatedEmpty()
        {
            FtlAssert.WorksScalar((int[] source) => source.FirstOrDefault(i => i > 50), new int[0]);
        }

        [TestMethod]
        public void FirstOrDefault_PredicatedNoHits()
        {
            FtlAssert.WorksScalar((int[] source) => source.FirstOrDefault(i => i > 50),
                Enumerable.Range(1, 30).ToArray());
        }

        [TestMethod]
        public void FirstOrDefault_ReferenceTypeNoHits()
        {
            FtlAssert.WorksScalar((object[] source) => source.FirstOrDefault(i => (int) i > 50),
                Enumerable.Range(1, 30).Select(i => (object) i).ToArray());
        }
    }
}
