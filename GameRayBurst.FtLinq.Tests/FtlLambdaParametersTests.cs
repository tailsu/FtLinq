using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests
{
    [TestClass]
    public class FtlLambdaParametersTests
    {
        [TestMethod, PerformanceTest]
        public void LambdaParameters_OneParam()
        {
            FtlAssert.Works((int[] source, int modulo) =>
                            source
                                .Where(i => i % modulo == 0)
                                .ToList(),
                                3, FtlAssert.TestData);
        }

        [TestMethod, PerformanceTest]
        public void LambdaParameters_TwoParams()
        {
            FtlAssert.Works((int[] source, int modulo, Func<int, string> selector) =>
                source
                    .Where(i => i % modulo == 0)
                    .Select(i => selector(i))
                    .ToList(),
                    3, i => i.ToString(), FtlAssert.TestData);
        }
    }
}
