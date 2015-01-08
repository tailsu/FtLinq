using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GameRayBurst.FtLinq.Tests
{
    [TestClass]
    public class ScratchPad
    {
        [TestMethod]
        public void Test()
        {
            var q = Ftl.Compile((Type t) => t.GetMethods().SelectMany(m => m.GetCustomAttributes(true)).ToList());
            var result = q(typeof (ScratchPad));
        }

        [TestMethod]
        public void TestOrdering()
        {
            Func<Type, IEnumerable<object>> q = t =>
                t.GetMethods()
                .OrderBy(m => m.Name)
                .ThenBy(m => m.CallingConvention)
                .ToList();
        }
    }
}
