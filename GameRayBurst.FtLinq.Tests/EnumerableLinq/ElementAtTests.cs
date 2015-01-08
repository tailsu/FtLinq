using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests.EnumerableLinq
{
    [TestClass]
    public class ElementAtTests
    {
        [TestMethod]
        public void ElementAt_Basic()
        {
            FtlAssert.WorksScalar((int[] source) => source.ElementAt(0), new[] { 1, 2, 3 });
            FtlAssert.WorksScalar((int[] source) => source.ElementAt(1), new[] { 1, 2, 3 });
            FtlAssert.WorksScalar((int[] source) => source.ElementAt(2), new[] { 1, 2, 3 });
        }

        [TestMethod]
        public void ElementAt_Empty()
        {
            FtlAssert.Throws((int[] source) => source.ElementAt(0), new int[0]);
            FtlAssert.Throws((int[] source) => source.ElementAt(1), new int[0]);
            FtlAssert.Throws((int[] source) => source.ElementAt(-1), new int[0]);
        }

        [TestMethod]
        public void ElementAt_OutOfRange()
        {
            FtlAssert.Throws((int[] source) => source.ElementAt(100), new[] { 1, 2 });
            FtlAssert.Throws((int[] source) => source.ElementAt(-1), new[] { 1, 2 });
        }

        [TestMethod]
        public void ElementAt_BreakFromNestedLoop()
        {
            FtlAssert.WorksScalar((int[] source) =>
                source
                .SelectMany(i => new[] { i, i, i })
                .ElementAt(4), new[] { 1, 2, 3 });
        }

        [TestMethod]
        public void ElementAtOrDefault_BreakFromNestedLoop()
        {
            FtlAssert.WorksScalar((int[] source) =>
                source
                .SelectMany(i => new[] { i, i, i })
                .ElementAtOrDefault(4), new[] { 1, 2, 3 });
        }

        [TestMethod]
        public void ElementAtOrDefault_Basic()
        {
            FtlAssert.WorksScalar((int[] source) => source.ElementAtOrDefault(5));
        }

        [TestMethod]
        public void ElementAtOrDefault_Empty()
        {
            FtlAssert.WorksScalar((int[] source) => source.ElementAtOrDefault(0), new int[0]);
            FtlAssert.WorksScalar((int[] source) => source.ElementAtOrDefault(1), new int[0]);
            FtlAssert.WorksScalar((int[] source) => source.ElementAtOrDefault(-1), new int[0]);
        }

        [TestMethod]
        public void ElementAtOrDefault_OutOfRange()
        {
            FtlAssert.WorksScalar((int[] source) => source.ElementAtOrDefault(100), new[] { 1, 2 });
            FtlAssert.WorksScalar((int[] source) => source.ElementAtOrDefault(-1), new[] { 1, 2 });
        }
    }
}
