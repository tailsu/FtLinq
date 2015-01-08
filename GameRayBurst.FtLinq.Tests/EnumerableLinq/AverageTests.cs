using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests.EnumerableLinq
{
    [TestClass]
    public class AverageTests
    {
        [TestMethod, PerformanceTest]
        public void Average_DoublesArray()
        {
            FtlAssert.WorksScalar(
                (double[] source) => source.Average(),
                FtlAssert.TestData.Select(i => (double) i).ToArray());
        }

        [TestMethod, PerformanceTest]
        public void Average_DoublesList()
        {
            FtlAssert.WorksScalar(
                (List<double> source) => source.Average(),
                FtlAssert.TestData.Select(i => (double) i).ToList());
        }

        [TestMethod, PerformanceTest]
        public void Average_DoublesEnumerable()
        {
            FtlAssert.WorksScalar(
                (IEnumerable<double> source) => source.Average(),
                FtlAssert.TestData.Select(i => (double) i));
        }

        [TestMethod]
        public void Average_Empty()
        {
            FtlAssert.Throws((int[] source) => source.Average(), new int[0]);
        }

        [TestMethod]
        public void Average_EmptyList()
        {
            FtlAssert.Throws((List<int> source) => source.Average(), new List<int>());
        }

        [TestMethod]
        public void Average_EmptyTypedEnumerable()
        {
            FtlAssert.Throws((IEnumerable<int> source) => source.Average(), new List<int>());
        }

        [TestMethod]
        public void Average_EmptyUntypedEnumerable()
        {
            FtlAssert.Throws((IEnumerable source) => source.Cast<int>().Average(), new List<int>());
        }

        [TestMethod, PerformanceTest]
        public void Average_DoublesProjection()
        {
            FtlAssert.WorksScalar(
                (double[] source) => source.Average(i => Math.Abs(50 - i)),
                FtlAssert.TestData.Select(i => (double) i).ToArray());
        }

        [TestMethod, PerformanceTest]
        public void Average_DoublesNullable()
        {
            FtlAssert.WorksScalar(
                (double?[] source) => source.Average(),
                FtlAssert.TestData.Select(i => (double?) i).ToArray());
        }

        [TestMethod, PerformanceTest]
        public void Average_DoublesNullableFromProjection()
        {
            FtlAssert.WorksScalar(
                (double[] source) => source.Average(d => (double?) d),
                FtlAssert.TestData.Select(i => (double) i).ToArray());
        }
    }
}
