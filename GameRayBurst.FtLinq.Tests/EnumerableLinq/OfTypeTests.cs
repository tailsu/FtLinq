using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests.EnumerableLinq
{
    [TestClass]
    public class OfTypeTests
    {
        [TestMethod]
        public void OfType_ReferenceType()
        {
            FtlAssert.Works((object[] source) =>
                source.OfType<string>().ToList(),
                FtlAssert.TestData.Select(i =>
            {
                if (i % 2 == 0)
                    return (object) i;
                return i.ToString();
            }).ToArray());
        }

        [TestMethod]
        public void OfType_ValueType()
        {
            FtlAssert.Works((object[] source) =>
                source.OfType<double>().ToList(),
                FtlAssert.TestData.Select(i =>
                {
                    if (i % 2 == 0)
                        return (object) i;
                    return (double) i;
                }).ToArray());
        }

        [TestMethod]
        public void OfType_Nullable()
        {
            FtlAssert.Works((int?[] source) =>
                source.OfType<int>().ToList(),
                FtlAssert.TestData.Select(i =>
                {
                    if (i % 2 == 0)
                        return (int?) i;
                    return null;
                }).ToArray());
        }
    }
}
