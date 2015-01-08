using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests.EnumerableLinq
{
    [TestClass]
    public class SkipTests
    {
        [TestMethod]
        public void Skip_None()
        {
            FtlAssert.Works((IEnumerable<int> source) => source.Skip(0).ToList(), FtlAssert.TestData);
        }

        [TestMethod]
        public void Skip_NothingToSkip()
        {
            FtlAssert.Works((IEnumerable<int> source) => source.Skip(10).ToList(), new int[0]);
        }

        [TestMethod]
        public void Skip_IncompleteSkip()
        {
            FtlAssert.Works((IEnumerable<int> source) => source.Skip(10).ToList(), Enumerable.Range(1, 5));
        }

        private static bool getSkipCountCalled;
        private static int GetSkipCount()
        {
            if (getSkipCountCalled) Assert.Fail("Skip expression evaluated multiple times");
            getSkipCountCalled = true;
            return 50;
        }

        [TestMethod]
        public void Skip_ComplexSkipExpression_EvaluatedOnce()
        {
            var query = Ftl.Compile((IEnumerable<int> source) => source.Skip(GetSkipCount()).ToList());
            query(FtlAssert.TestData);
        }

        [TestMethod]
        public void Skip_Simple()
        {
            FtlAssert.Works((IEnumerable<int> source) => source.Skip(20).ToList(), FtlAssert.TestData);
        }

        [TestMethod, PerformanceTest]
        public void Skip_PrefixedMethods()
        {
            FtlAssert.Works((int[] source) => source.Where(i => i % 4 != 1).Skip(20).ToList(), FtlAssert.TestData);
        }

        public void Skip_MultipleSkips_CountersNotThrashed()
        {
            FtlAssert.Works((IEnumerable<int> source) => source.Skip(1).Skip(2).Skip(3).ToList(), FtlAssert.TestData);
        }
    }
}
