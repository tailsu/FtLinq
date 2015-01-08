using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GameRayBurst.FtLinq.Tests
{
    [TestClass]
    public class DistinctTests
    {
        [TestMethod]
        public void Distinct_Basic()
        {
            FtlAssert.Works((int[] source) => source.Distinct().ToList());
        }

        [TestMethod]
        public void Distinct_Empty()
        {
            FtlAssert.Works((int[] source) => source.Distinct().ToList(), new int[0]);
        }

        [TestMethod]
        public void Distinct_Comparer()
        {
            FtlAssert.Works((int[] source) => source
                .Select(i => ToBinaryString(i, 'a', 'A'))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList());
        }

        [TestMethod]
        public void Distinct_KeySelector()
        {
            FtlAssert.Works((int[] source) => source
                .Distinct(i => i % 2)
                .ToList());
        }

        [TestMethod]
        public void Distinct_KeySelectorAndComparer()
        {
            FtlAssert.Works((int[] source) => source
                .Distinct(i => ToBinaryString(i, 'a', 'A'), StringComparer.OrdinalIgnoreCase)
                .ToList());
        }

        [TestMethod]
        public void Distinct_ReturnsFirstElement()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void DistinctWithKeySelector_LibraryImplementationBasic()
        {
            var data = FtlEnumerable.Range(3, 100).ToArray();
            var result = data.Distinct(i => i % 2).ToList();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(3, result[0]);
            Assert.AreEqual(4, result[1]);
        }

        [TestMethod]
        public void DistinctWithKeySelector_LibraryImplementationComparer()
        {
            var data = FtlEnumerable.Range(1, 128).ToArray();
            var result = data.Distinct(i => ToBinaryString(i, 'a', 'A'), StringComparer.OrdinalIgnoreCase).ToList();
            Assert.AreEqual(8, result.Count);
            for (int i = 0; i < result.Count; ++i)
                Assert.IsTrue((result[i] & (result[i]-1)) == 0);
        }

        [TestMethod]
        public void DistinctWithKeySelector_LibraryImplementation_ReturnsFirstElement()
        {
            Assert.Inconclusive();
        }

        private static string ToBinaryString(int a, char zero, char one)
        {
            if (a == 0)
                return new String(zero, 1);

            var result = new StringBuilder();
            while (a != 0)
            {
                result.Append((a & 1) != 0 ? one : zero);
                a >>= 1;
            }
            return result.ToString();
        }
    }
}
