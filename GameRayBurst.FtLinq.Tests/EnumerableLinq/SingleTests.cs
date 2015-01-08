using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests.EnumerableLinq
{
    [TestClass]
    public class SingleTests
    {
        [TestMethod]
        public void Single_Basic()
        {
            FtlAssert.WorksScalar((int[] source) => source.Single(), new[] { 1 });
        }

        [TestMethod]
        public void Single_Empty()
        {
            FtlAssert.Throws((int[] source) => source.Single(), new int[0]);
        }

        [TestMethod]
        public void Single_MoreThanOne()
        {
            FtlAssert.Throws((int[] source) => source.Single(), new[] { 1, 2 });
        }

        [TestMethod, PerformanceTest]
        public void Single_Predicated()
        {
            FtlAssert.WorksScalar((int[] source) => source.Single(i => i > 0), new[] { 1 });
        }

        [TestMethod]
        public void Single_PredicatedEmpty()
        {
            FtlAssert.Throws((int[] source) => source.Single(i => i > 50), new int[0]);
        }

        [TestMethod]
        public void Single_PredicatedNoHits()
        {
            FtlAssert.Throws((int[] source) => source.Single(i => i > 50), new[] { 10, 20, 30 });
        }

        [TestMethod]
        public void Single_PredicatedMultipleHits()
        {
            FtlAssert.Throws((int[] source) => source.Single(i => i > 50), new[] { 100, 200, 300 });
        }

        [TestMethod]
        public void Single_BreakFromNestedLoop()
        {
            FtlAssert.WorksScalar((int[] source) =>
                source
                .SelectMany(i => new[] { i })
                .Single(), new[] { 1 });
        }

        [TestMethod]
        public void SingleOrDefault_BreakFromNestedLoop()
        {
            FtlAssert.WorksScalar((int[] source) =>
                source
                .SelectMany(i => new[] { i })
                .SingleOrDefault(), new[] { 1 });
        }

        [TestMethod]
        public void SingleOrDefault_Basic()
        {
            FtlAssert.WorksScalar((int[] source) => source.SingleOrDefault(), new[] { 1 });
        }

        [TestMethod]
        public void SingleOrDefault_Empty()
        {
            FtlAssert.WorksScalar((int[] source) => source.SingleOrDefault(), new int[0]);
        }

        [TestMethod]
        public void SingleOrDefault_MoreThanOne()
        {
            FtlAssert.Throws((int[] source) => source.SingleOrDefault(), new[] { 1, 2 });
        }

        [TestMethod, PerformanceTest]
        public void SingleOrDefault_Predicated()
        {
            FtlAssert.WorksScalar((int[] source) => source.SingleOrDefault(i => i > 0), new[] { 10 });
        }

        [TestMethod]
        public void SingleOrDefault_PredicatedEmpty()
        {
            FtlAssert.WorksScalar((int[] source) => source.SingleOrDefault(i => i > 50), new int[0]);
        }

        [TestMethod]
        public void SingleOrDefault_PredicatedNoHits()
        {
            FtlAssert.WorksScalar((int[] source) => source.SingleOrDefault(i => i > 50),
                new[] { 10, 20, 30 });
        }

        [TestMethod]
        public void SingleOrDefault_PredicatedMultipleHits()
        {
            FtlAssert.Throws((int[] source) => source.SingleOrDefault(i => i > 50), new[] { 100, 200 });
        }

        [TestMethod]
        public void SingleOrDefault_ReferenceTypeNoHits()
        {
            FtlAssert.WorksScalar((object[] source) => source.SingleOrDefault(i => (int)i > 50),
                Enumerable.Range(1, 30).Select(i => (object)i).ToArray());
        }
    }
}
