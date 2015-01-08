using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GameRayBurst.FtLinq.Tests
{
    [TestClass]
    public class GenerateTests
    {
        [TestMethod]
        public void FtlEnumerableGenerate_NonEmpty()
        {
            int i = 1;
            foreach (var value in FtlEnumerable.Generate(() => i * 2, 100))
            {
                Assert.AreEqual(i * 2, value);
                i++;
            }
        }

        [TestMethod]
        public void FtlEnumerableGenerate_Empty()
        {
            int i = 0;
            var list = FtlEnumerable.Generate(() => 5 / i, 0);
            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        public void FtlEnumerableGenerate_IndexingNonEmpty()
        {
            int count = 0;
            foreach (var value in FtlEnumerable.Generate(i => i + 10, 100))
            {
                Assert.AreEqual(count + 10, value);
                count++;
            }
        }

        private class CountsInstantiations
        {
            [ThreadStatic]
            public static int Count;

            public CountsInstantiations()
            {
                Count++;
            }
        }

        [TestMethod]
        public void FtlEnumerableGenerate_CheckDelegateIsCalledOnlyOncePerElement()
        {
            CountsInstantiations.Count = 0;
            var list = FtlEnumerable.Generate(() => new CountsInstantiations(), 123).ToList();
            Assert.AreEqual(123, CountsInstantiations.Count);
        }

        [TestMethod]
        public void FtlEnumerableGenerate_Indexing_CheckDelegateIsCalledOnlyOncePerElement()
        {
            CountsInstantiations.Count = 0;
            var list = FtlEnumerable.Generate(i => new CountsInstantiations(), 123).ToList();
            Assert.AreEqual(123, CountsInstantiations.Count);
        }

        [TestMethod]
        public void Generate_NonEmpty()
        {
            FtlAssert.WorksScalar(() => FtlEnumerable.Generate(() => 5, 10).Sum());
        }

        [TestMethod]
        public void Generate_Empty()
        {
            FtlAssert.WorksScalar((Generator<int> source) => source.Sum(), FtlEnumerable.Generate(() => 5, 0));
        }

        [TestMethod]
        public void Generate_Empty_ThrowOnEmptySequenceWorks()
        {
            FtlAssert.Throws(i => FtlEnumerable.Generate(() => 5, i).Average(), 0);
        }

        [TestMethod]
        public void Generate_IndexingNonEmpty()
        {
            FtlAssert.WorksScalar(() => FtlEnumerable.Generate(i => i * 10, 10).Sum());
        }

        [TestMethod]
        public void Generate_IndexingEmpty()
        {
            FtlAssert.WorksScalar((IndexingGenerator<int> source) => source.Sum(), FtlEnumerable.Generate(i => 5, 0));
        }

        [TestMethod]
        public void Generate_IndexingEmpty_ThrowOnEmptySequenceWorks()
        {
            FtlAssert.Throws(n => FtlEnumerable.Generate(i => 5, n).Average(), 0);
        }

        [TestMethod, PerformanceTest]
        public void Generate_Preallocate()
        {
            FtlAssert.Works(_ => FtlEnumerable.Generate(() => 5, 10000).ToList());
        }
    }
}
