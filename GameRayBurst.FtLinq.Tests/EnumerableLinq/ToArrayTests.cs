using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GameRayBurst.FtLinq.Tests.EnumerableLinq
{
    [TestClass]
    public class ToArrayTests
    {
        [TestMethod]
        public void ToArray_Empty()
        {
            FtlAssert.Works((int[] source) => source.ToArray(), new int[0]);
        }

        [TestMethod]
        public void ToArray_Basic()
        {
            FtlAssert.Works((IEnumerable<int> source) => source.ToArray(), Enumerable.Range(1, 10000));
        }

        [TestMethod]
        public void ToArray_Pow2Size()
        {
            FtlAssert.Works((IEnumerable<int> source) => source.ToArray(), Enumerable.Range(1, 1024));
        }

        [TestMethod]
        public void ToArray_Small()
        {
            FtlAssert.Works((int[] source) => source.ToArray(), new[] { 1 });
        }

        [TestMethod]
        [Ignore] // throws OOM probably because the test runner is 32-bit
        public void ToArray_VeryLarge()
        {
            Func<IEnumerable<char>, char[]> query = Ftl.Compile((IEnumerable<char> source) => source.ToArray());
            const int largeCount = 1024*1024*1100;
            var result = query(Enumerable.Repeat('a', largeCount));
            Assert.AreEqual(largeCount, result.Length);
        }

        [TestMethod]
        public void Diagnostics_ToArray()
        {
            DiagnosticEventArgs diagnostic = null;
            EventHandler<DiagnosticEventArgs> handler = (o, e) => diagnostic = e;

            var oldValue = FtlConfiguration.PrintDiagnosticInfo;
            FtlConfiguration.PrintDiagnosticInfo = true;
            FtlConfiguration.Diagnostic += handler;
            Ftl.Compile((int[] diagnosed) => diagnosed.ToArray());
            FtlConfiguration.Diagnostic -= handler;
            FtlConfiguration.PrintDiagnosticInfo = oldValue;

            Assert.IsNotNull(diagnostic);
            Assert.AreEqual("ToArray", ((MethodCallExpression) diagnostic.Origin).Method.Name);
        }
    }
}
