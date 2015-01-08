using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests
{
    [TestClass]
    public class FtlCompileTests
    {
        [TestMethod]
        [DontDecompile]
        public void Compile()
        {
            Assert.AreSame(typeof(List<int>), Ftl.Compile(() => Enumerable.Range(1, 10).ToList()).Method.ReturnType);
            Assert.AreSame(typeof(List<int>), Ftl.Compile((int[] source) => source.ToList()).Method.ReturnType);
            Assert.AreSame(typeof(List<int>), Ftl.Compile((int[] source, int a) => source.ToList()).Method.ReturnType);
            Assert.AreSame(typeof(List<int>), Ftl.Compile((int[] source, int a, int b) => source.ToList()).Method.ReturnType);
            Assert.AreSame(typeof(List<int>), Ftl.Compile((int[] source, int a, int b, int c) => source.ToList()).Method.ReturnType);
            Assert.AreSame(typeof(List<int>), Ftl.Compile((int[] source, int a, int b, int c, int d) => source.ToList()).Method.ReturnType);
            Assert.AreSame(typeof(List<int>), Ftl.Compile((int[] source, int a, int b, int c, int d, int e) => source.ToList()).Method.ReturnType);

            Assert.AreSame(typeof(int), Ftl.Compile(() => new int[0].Sum()).Method.ReturnType);
            Assert.AreSame(typeof(int), Ftl.Compile((int[] source) => source.Sum()).Method.ReturnType);
            Assert.AreSame(typeof(int), Ftl.Compile((int[] source, int a) => source.Sum()).Method.ReturnType);
            Assert.AreSame(typeof(int), Ftl.Compile((int[] source, int a, int b) => source.Sum()).Method.ReturnType);
            Assert.AreSame(typeof(int), Ftl.Compile((int[] source, int a, int b, int c) => source.Sum()).Method.ReturnType);
            Assert.AreSame(typeof(int), Ftl.Compile((int[] source, int a, int b, int c, int d) => source.Sum()).Method.ReturnType);
            Assert.AreSame(typeof(int), Ftl.Compile((int[] source, int a, int b, int c, int d, int e) => source.Sum()).Method.ReturnType);

            Assert.IsTrue(typeof(IEnumerable<IGrouping<int, int>>).IsAssignableFrom(Ftl.Compile(() => new int[0].GroupBy(_ => _)).Method.ReturnType));
            Assert.IsTrue(typeof(IEnumerable<IGrouping<int, int>>).IsAssignableFrom(Ftl.Compile((int[] source) => source.GroupBy(_ => _)).Method.ReturnType));
            Assert.IsTrue(typeof(IEnumerable<IGrouping<int, int>>).IsAssignableFrom(Ftl.Compile((int[] source, int a) => source.GroupBy(_ => _)).Method.ReturnType));
            Assert.IsTrue(typeof(IEnumerable<IGrouping<int, int>>).IsAssignableFrom(Ftl.Compile((int[] source, int a, int b) => source.GroupBy(_ => _)).Method.ReturnType));
            Assert.IsTrue(typeof(IEnumerable<IGrouping<int, int>>).IsAssignableFrom(Ftl.Compile((int[] source, int a, int b, int c) => source.GroupBy(_ => _)).Method.ReturnType));
            Assert.IsTrue(typeof(IEnumerable<IGrouping<int, int>>).IsAssignableFrom(Ftl.Compile((int[] source, int a, int b, int c, int d) => source.GroupBy(_ => _)).Method.ReturnType));
            Assert.IsTrue(typeof(IEnumerable<IGrouping<int, int>>).IsAssignableFrom(Ftl.Compile((int[] source, int a, int b, int c, int d, int e) => source.GroupBy(_ => _)).Method.ReturnType));
            Assert.IsTrue(typeof(IEnumerable<IGrouping<int, int>>).IsAssignableFrom(Ftl.Compile((int[] source, int a, int b, int c, int d, int e, int f) => source.GroupBy(_ => _)).Method.ReturnType));
            Assert.IsTrue(typeof(IEnumerable<IGrouping<int, int>>).IsAssignableFrom(Ftl.Compile((int[] source, int a, int b, int c, int d, int e, int f, int g) => source.GroupBy(_ => _)).Method.ReturnType));
            Assert.IsTrue(typeof(IEnumerable<IGrouping<int, int>>).IsAssignableFrom(Ftl.Compile((int[] source, int a, int b, int c, int d, int e, int f, int g, int h) => source.GroupBy(_ => _)).Method.ReturnType));
            Assert.IsTrue(typeof(IEnumerable<IGrouping<int, int>>).IsAssignableFrom(Ftl.Compile((int[] source, int a, int b, int c, int d, int e, int f, int g, int h, int i) => source.GroupBy(_ => _)).Method.ReturnType));
            Assert.IsTrue(typeof(IEnumerable<IGrouping<int, int>>).IsAssignableFrom(Ftl.Compile((int[] source, int a, int b, int c, int d, int e, int f, int g, int h, int i, int j) => source.GroupBy(_ => _)).Method.ReturnType));
        }
    }
}
