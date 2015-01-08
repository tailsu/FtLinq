using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests.EnumerableLinq
{
    [TestClass]
    public class ToDictionaryTests
    {
        [TestMethod]
        public void ToDictionary_SimpleKeySelector()
        {
            FtlAssert.Works((int[] source) => source.ToDictionary(n => n));
        }

        [TestMethod]
        public void ToDictionary_EqualityComparer()
        {
            FtlAssert.Works((int[] source) => source.ToDictionary(n => n.ToString(), StringComparer.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void ToDictionary_EqualityComparer_DuplicateKeys()
        {
            FtlAssert.Throws((string[] source) => source.ToDictionary(n => n, StringComparer.OrdinalIgnoreCase), new[] { "a", "A" });
        }

        [TestMethod]
        public void ToDictionary_SameKeyValueTypes()
        {
            FtlAssert.Works((int[] source) => source.ToDictionary(n => n, n => n * 2));
        }

        [TestMethod]
        public void ToDictionary_DifferentKeyValueTypes()
        {
            FtlAssert.Works((int[] source) => source.ToDictionary(n => n, n => n.ToString()));
        }

        [TestMethod]
        public void ToDictionary_SameKeyValues_Throws()
        {
            FtlAssert.Throws((int[] source) => source.ToDictionary(n => 1), new[] { 1, 2, 3 });
        }

        [TestMethod]
        public void ToDictionary_KeyValueSelectorsAndEqualityComparer()
        {
            FtlAssert.Works((int[] source) => source.ToDictionary(n => n.ToString(), n => n * 2, StringComparer.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void ToDictionary_KeyValueSelectorsAndEqualityComparer_DuplicateKeys()
        {
            FtlAssert.Throws((string[] source) => source.ToDictionary(n => n, n => n.Reverse(), StringComparer.OrdinalIgnoreCase), new[] { "a", "A" });
        }
    }
}
