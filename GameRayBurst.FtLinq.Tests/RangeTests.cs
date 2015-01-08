using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GameRayBurst.FtLinq.Tests
{
    [TestClass]
    public class RangeTests
    {
        [TestMethod]
        public void FtlEnumerableRange_NonEmpty()
        {
            int i = 1;
            foreach (var value in FtlEnumerable.Range(1, 100))
            {
                Assert.AreEqual(i, value);
                i++;
            }
        }

        [TestMethod]
        public void FtlEnumerableRange_Empty()
        {
            var list = FtlEnumerable.Range(100, 0).ToList();
            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        public void Range_NonEmpty()
        {
            FtlAssert.WorksScalar((Range source) => source.Sum(), FtlEnumerable.Range(100, 200));
        }

        [TestMethod]
        public void Range_Empty_ThrowOnEmptySequenceWorks()
        {
            FtlAssert.Throws((Range source) => source.Average(), FtlEnumerable.Range(1, 0));
        }

        [TestMethod, PerformanceTest]
        public void Range_Preallocate()
        {
            FtlAssert.Works(_ => FtlEnumerable.Range(0, 10000).ToList());
        }
    }
}
