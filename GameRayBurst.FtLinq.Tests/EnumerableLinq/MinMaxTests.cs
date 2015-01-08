using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests.EnumerableLinq
{
    [TestClass]
    public class MinMaxTests
    {
        [TestMethod, PerformanceTest]
        public void Min_DoublesArray()
        {
            FtlAssert.WorksScalar(
                (double[] source) => source.Min(),
                FtlAssert.TestData.Select(i => (double) i).ToArray());
        }

        [TestMethod, PerformanceTest]
        public void Min_DoublesList()
        {
            FtlAssert.WorksScalar(
                (List<double> source) => source.Min(),
                FtlAssert.TestData.Select(i => (double) i).ToList());
        }

        [TestMethod, PerformanceTest]
        public void Min_DoublesEnumerable()
        {
            FtlAssert.WorksScalar(
                (IEnumerable<double> source) => source.Min(),
                FtlAssert.TestData.Select(i => (double) i));
        }

        [TestMethod]
        public void Min_Empty()
        {
            FtlAssert.Throws((int[] source) => source.Min(), new int[0]);
        }

        [TestMethod]
        public void Min_EmptyList()
        {
            FtlAssert.Throws((List<int> source) => source.Min(), new List<int>());
        }

        [TestMethod]
        public void Min_EmptyTypedEnumerable()
        {
            FtlAssert.Throws((IEnumerable<int> source) => source.Min(), new List<int>());
        }

        [TestMethod]
        public void Min_EmptyUntypedEnumerable()
        {
            FtlAssert.Throws((IEnumerable source) => source.Cast<int>().Min(), new List<int>());
        }

        [TestMethod, PerformanceTest]
        public void Min_DoublesProjection()
        {
            FtlAssert.WorksScalar(
                (double[] source) => source.Min(i => Math.Abs(50 - i)),
                FtlAssert.TestData.Select(i => (double) i).ToArray());
        }

        [TestMethod, PerformanceTest]
        public void Min_DoublesNullable()
        {
            FtlAssert.WorksScalar(
                (double?[] source) => source.Min(),
                FtlAssert.TestData.Select(i => (double?) i).ToArray());
        }

        [TestMethod, PerformanceTest]
        public void Min_DoublesNullableFromProjection()
        {
            FtlAssert.WorksScalar(
                (double[] source) => source.Min(d => (double?) d),
                FtlAssert.TestData.Select(i => (double) i).ToArray());
        }

        [TestMethod, PerformanceTest]
        public void Max_Doubles()
        {
            FtlAssert.WorksScalar(
                (double[] source) => source.Max(),
                FtlAssert.TestData.Select(i => (double) i).ToArray());
        }

        [TestMethod]
        public void Max_Empty()
        {
            FtlAssert.Throws((int[] source) => source.Max(), new int[0]);
        }

        [TestMethod, PerformanceTest]
        public void Max_DoublesProjection()
        {
            FtlAssert.WorksScalar(
                (double[] source) => source.Max(i => -Math.Abs(50 - i)),
                FtlAssert.TestData.Select(i => (double) i).ToArray());
        }

        [TestMethod, PerformanceTest]
        public void Max_DoublesNullable()
        {
            FtlAssert.WorksScalar(
                (double?[] source) => source.Max(),
                FtlAssert.TestData.Select(i => (double?) i).ToArray());
        }

        [TestMethod, PerformanceTest]
        public void Max_DoublesNullableFromProjection()
        {
            FtlAssert.WorksScalar(
                (double[] source) => source.Max(d => (double?) d),
                FtlAssert.TestData.Select(i => (double) i).ToArray());
        }

        [TestMethod]
        public void Max_EmptyList()
        {
            FtlAssert.Throws((List<int> source) => source.Max(), new List<int>());
        }

        [TestMethod]
        public void Max_EmptyTypedEnumerable()
        {
            FtlAssert.Throws((IEnumerable<int> source) => source.Max(), new List<int>());
        }

        [TestMethod]
        public void Max_EmptyUntypedEnumerable()
        {
            FtlAssert.Throws((IEnumerable source) => source.Cast<int>().Max(), new List<int>());
        }

        [TestMethod, PerformanceTest]
        public void Max_DoublesList()
        {
            FtlAssert.WorksScalar(
                (List<double> source) => source.Max(),
                FtlAssert.TestData.Select(i => (double) i).ToList());
        }

        [TestMethod, PerformanceTest]
        public void Max_DoublesEnumerable()
        {
            FtlAssert.WorksScalar(
                (IEnumerable<double> source) => source.Max(),
                FtlAssert.TestData.Select(i => (double) i));
        }
    }
}
