using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests.EnumerableLinq
{
    [TestClass]
    public class TakeTests
    {
        [TestMethod]
        public void Take_None()
        {
            FtlAssert.Works((IEnumerable<int> source) => source.Take(0).ToList(), FtlAssert.TestData);
        }

        [TestMethod]
        public void Take_NothingToTake()
        {
            FtlAssert.Works((IEnumerable<int> source) => source.Take(10).ToList(), new int[0]);
        }

        [TestMethod]
        public void Take_IncompleteTake()
        {
            FtlAssert.Works((IEnumerable<int> source) => source.Take(10).ToList(), Enumerable.Range(1, 5));
        }

        private static bool getTakeCountCalled;
        private static int GetTakeCount()
        {
            if (getTakeCountCalled) Assert.Fail("Take expression evaluated multiple times");
            getTakeCountCalled = true;
            return 50;
        }

        [TestMethod]
        public void Take_ComplexTakeExpression_EvaluatedOnce()
        {
            var query = Ftl.Compile((IEnumerable<int> source) => source.Take(GetTakeCount()).ToList());
            query(FtlAssert.TestData);
        }

        [TestMethod]
        public void Take_Simple()
        {
            FtlAssert.Works((IEnumerable<int> source) => source.Take(20).ToList(), FtlAssert.TestData);
        }

        [TestMethod, PerformanceTest]
        public void Take_PrefixedMethods()
        {
            FtlAssert.Works((int[] source) => source.Where(i => i % 4 != 1).Take(50).ToList(), FtlAssert.TestData);
        }

        public void Take_MultipleTakes_CountersNotThrashed()
        {
            FtlAssert.Works((IEnumerable<int> source) => source.Take(1).Take(2).Take(3).ToList(), FtlAssert.TestData);
        }
    }
}
