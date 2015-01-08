using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests
{
    [TestClass]
    public class RepeatTests
    {
        [TestMethod]
        public void FtlEnumerableRepeat_NonEmpty()
        {
            int i = 0;
            foreach (var value in FtlEnumerable.Repeat(10.0, 50))
            {
                Assert.AreEqual(10.0, value);
                i++;
            }
            Assert.AreEqual(i, 50);
        }

        [TestMethod]
        public void FtlEnumerableRepeat_Empty()
        {
            var list = FtlEnumerable.Repeat(10.0, 0).ToList();
            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        public void Repeat_NonEmpty()
        {
            FtlAssert.WorksScalar(
                (Repetition<int> source) => source.Average(),
                FtlEnumerable.Repeat(10, 10));
        }

        [TestMethod]
        public void Repeat_Empty()
        {
            FtlAssert.WorksScalar(
                (Repetition<int> source) => source.Sum(),
                FtlEnumerable.Repeat(10, 0));
        }

        [TestMethod]
        public void Repeat_Empty_ThrowOnEmptySequenceWorks()
        {
            FtlAssert.Throws((Repetition<int> source) => source.Average(), FtlEnumerable.Repeat(10, 0));
        }

        [TestMethod]
        public void Repeat_NoParameter()
        {
            FtlAssert.WorksScalar(() => FtlEnumerable.Repeat(10.0, 100).Sum());
        }

        [TestMethod, PerformanceTest]
        public void Range_Preallocate()
        {
            FtlAssert.Works(_ => FtlEnumerable.Repeat(0, 10000).ToList());
        }
    }
}
