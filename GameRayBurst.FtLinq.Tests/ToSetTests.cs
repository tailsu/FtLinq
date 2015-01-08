using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests
{
    [TestClass]
    public class ToSetTests
    {
        [TestMethod]
        public void ToSet_Empty()
        {
            FtlAssert.Works((int[] source) => source.ToSet(), new int[0]);
        }

        [TestMethod]
        public void ToSet_Basic()
        {
            FtlAssert.Works((int[] source) => source.ToSet());
        }

        [TestMethod]
        public void EnumerableToSet_Basic()
        {
            var set = Enumerable.Range(1, 100).ToSet();
            for (int i = 1; i <= 100; ++i)
                Assert.IsTrue(set.Contains(i));
            Assert.AreEqual(100, set.Count);
        }
    }
}
