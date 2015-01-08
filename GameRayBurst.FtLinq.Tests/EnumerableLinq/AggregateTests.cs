using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GameRayBurst.FtLinq.Tests.EnumerableLinq
{
    [TestClass]
    public class AggregateTests
    {
        [TestMethod]
        public void Aggregate_EmptyNoSeed_Throws()
        {
            FtlAssert.Throws((int[] source) => source.Aggregate((aggr, item) => aggr + item), new int[0]);
        }

        [TestMethod, PerformanceTest]
        public void Aggregate_NoSeed()
        {
            FtlAssert.WorksScalar((int[] source) => source.Aggregate((aggr, item) => aggr + item));
        }

        [TestMethod]
        public void Aggregate_SeededEmpty()
        {
            var query = Ftl.Compile((int[] source) => source.Aggregate(123, (aggr, item) => aggr + item));
            var result = query(new int[0]);
            Assert.AreEqual(123, result);
        }

        [TestMethod, PerformanceTest]
        public void Aggregate_Seeded()
        {
            FtlAssert.WorksScalar((int[] source) => source.Aggregate(50, (aggr, item) => aggr + item));
        }

        [TestMethod]
        public void Aggregate_SeededWithSelectorEmpty()
        {
            var query = Ftl.Compile((int[] source) => source.Aggregate(123, (aggr, item) => aggr + item, aggr => aggr - 23));
            var result = query(new int[0]);
            Assert.AreEqual(100, result);
        }

        [TestMethod, PerformanceTest]
        public void Aggregate_SeededWithSelector()
        {
            FtlAssert.WorksScalar((int[] source) => source.Aggregate(50, (aggr, item) => aggr + item, aggr => 10 * aggr));
        }
    }
}
