using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests.EnumerableLinq
{
    [TestClass]
    public class AllTests
    {
        [TestMethod]
        public void All_Empty()
        {
            FtlAssert.WorksScalar((int[] source) => source.All(i => true), new[] {0});
        }

        [TestMethod, PerformanceTest]
        public void All_AllPass()
        {
            FtlAssert.WorksScalar((int[] source) => source.All(i => i > -1), FtlAssert.TestData);
        }

        [TestMethod, PerformanceTest]
        public void All_SomeDontPass()
        {
            FtlAssert.WorksScalar((int[] source) => source.All(i => i < 50), FtlAssert.TestData);
        }

        [TestMethod, PerformanceTest]
        public void All_IdentityPredicate()
        {
            FtlAssert.WorksScalar((int[] source) => source.All(i => true), FtlAssert.TestData);
        }
    }
}
