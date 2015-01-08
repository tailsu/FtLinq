using System.Collections;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests.EnumerableLinq
{
    [TestClass]
    public class CastTests
    {
        [TestMethod, PerformanceTest]
        public void Cast_Boxed()
        {
            var list = new ArrayList(FtlAssert.TestData);
            FtlAssert.Works((IEnumerable source) => source.Cast<int>().ToList(), list);
        }

        [TestMethod, PerformanceTest]
        public void Cast_Downcast()
        {
            FtlAssert.Works(
                (object[] source) => source.Cast<string>().ToList(),
                FtlAssert.TestData.Select(i => (object) i.ToString()).ToArray());
        }
    }
}
