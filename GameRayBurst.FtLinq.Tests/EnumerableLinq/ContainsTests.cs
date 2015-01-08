using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GameRayBurst.FtLinq.Tests.EnumerableLinq
{
    [TestClass]
    public class ContainsTests
    {
        [TestMethod]
        public void Contains_Empty()
        {
            FtlAssert.WorksScalar((int[] source) => source.Contains(12), new int[0]);
        }

        [TestMethod]
        public void Contains_Positive()
        {
            FtlAssert.WorksScalar((string[] source) => source.Contains("foo"), new[] { "abc", "foo", "bar" });
        }

        [TestMethod]
        public void Contains_Negative()
        {
            FtlAssert.WorksScalar((string[] source) => source.Contains("foo"), new[] { "1", "2", "3" });
        }

        [TestMethod]
        public void Contains_EqualityComparer_Negative()
        {
            FtlAssert.WorksScalar((string[] source) => source.Contains("abc", StringComparer.InvariantCultureIgnoreCase),
                new[] { "abc1" });
        }

        [TestMethod]
        public void Contains_EqualityComparer_Positive()
        {
            FtlAssert.WorksScalar((string[] source) => source.Contains("abc", StringComparer.InvariantCultureIgnoreCase),
                new[] { "ABC" });
        }

        [TestMethod, PerformanceTest]
        public void Contains_Ints_Negative()
        {
            FtlAssert.WorksScalar((int[] source) => source.Contains(-100));
        }

        private static bool getTestValueCalled = false;
        private static string GetTestValue()
        {
            if (getTestValueCalled) Assert.Fail();
            getTestValueCalled = true;
            return "test";
        }

        [TestMethod]
        public void Contains_TestValueExpressionEvaluatedOnlyOnce()
        {
            var query = Ftl.Compile((string[] source) => source.Contains(GetTestValue()));
            query(new[] {"a", "b", "c"});
            Assert.IsTrue(getTestValueCalled);
        }
    }
}
