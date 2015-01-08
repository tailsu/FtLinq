using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameRayBurst.FtLinq.Recompiler.Util;
using System.Linq;

namespace GameRayBurst.FtLinq.Tests
{
    [TestClass]
    public class FtlOrderByTests
    {
        public class ReverseComparer : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                return y - x;
            }
        }

        private void Run(Func<IEnumerable<int>, IEnumerable<int>> referenceTransform, Func<IEnumerable<int>, IEnumerable<int>> ftlTransform)
        {
            var data = FtlAssert.RandomData.ToArray();
            var reference = referenceTransform(data).ToArray();
            var ftlSort = ftlTransform(data).ToArray();
            CollectionAssert.AreEqual(reference, ftlSort);
        }

        [TestMethod]
        public void FtlOrderBy_IdentitySort()
        {
            Run(data => data.OrderBy(_ => _),
                data => data.FtlOrderBy(_ => _));

            Run(data => data.OrderBy(_ => _),
                data => data.FtlSort());
        }

        [TestMethod]
        public void FtlOrderBy_IdentitySortAndComparer()
        {
            Run(data => data.OrderBy(_ => _, new ReverseComparer()),
                data => data.FtlOrderBy(_ => _, new ReverseComparer()));

            Run(data => data.OrderBy(_ => _, new ReverseComparer()),
                data => data.FtlSort(new ReverseComparer()));
        }

        [TestMethod]
        public void FtlOrderBy_SimpleSingleKeySort()
        {
            // exercise simple single key fast path
            Run(data => data.OrderBy(i => -i),
                data => data.FtlOrderBy(i => -i));
        }

        [TestMethod]
        public void FtlOrderBy_SingleKeyAndComparer()
        {
            Run(data => data.OrderBy(i => -i, new ReverseComparer()),
                data => data.FtlOrderBy(i => -i, new ReverseComparer()));
        }

        [TestMethod]
        public void FtlOrderBy_ThenBy()
        {
            Run(data => data.OrderBy(i => i % 10).ThenBy(i => i / 10),
                data => data.FtlOrderBy(i => i % 10).ThenBy(i => i / 10));
        }

        [TestMethod]
        public void FtlOrderBy_ThenByAndComparer()
        {
            Run(data => data.OrderBy(i => i % 10, new ReverseComparer()).ThenBy(i => i / 10, new ReverseComparer()),
                data => data.FtlOrderBy(i => i % 10, new ReverseComparer()).ThenBy(i => i / 10, new ReverseComparer()));
        }

        [TestMethod]
        public void FtlOrderByDescending_ThenByDescending()
        {
            Run(data => data.OrderByDescending(i => i % 10).ThenByDescending(i => i / 10),
                data => data.FtlOrderByDescending(i => i % 10).ThenByDescending(i => i / 10));
        }

        [TestMethod]
        public void FtlOrderBy_Mix()
        {
            Run(data => data.OrderBy(i => i % 10).ThenByDescending(i => i / 10, new ReverseComparer()).ThenBy(i => -i).ThenByDescending(i => 100000 * i),
                data => data.FtlOrderBy(i => i % 10).ThenByDescending(i => i / 10, new ReverseComparer()).ThenBy(i => -i).ThenByDescending(i => 100000 * i));
        }

        [TestMethod]
        public void FtlOrderBy_NonGenericIComparable()
        {
            Run(data => data.OrderBy(i => (object)i).ThenBy(i => (object)(-i)),
                data => data.FtlOrderBy(i => (object)i).ThenBy(i => (object)(-i)));
        }

        [TestMethod]
        public void FtlOrderBy_IComparable()
        {
            Run(data => data.OrderBy(i => (IComparable<int>)i).ThenBy(i => (IComparable<int>)(-i)),
                data => data.FtlOrderBy(i => (IComparable<int>)i).ThenBy(i => (IComparable<int>)(-i)));
        }

        [TestMethod]
        public void FtlOrderBy_PrimitiveKeyTypes()
        {
            Run(data => data.OrderBy(i => (byte)i).ThenBy(i => (sbyte)i)
                .ThenBy(i => (short)i).ThenBy(i => (ushort)i)
                .ThenBy(i => i).ThenBy(i => (uint)i)
                .ThenBy(i => (float)i).ThenBy(i => (double)i)
                .ThenBy(i => (decimal)i)
                .ThenBy(i => i % 2 == 0)
                .ThenBy(i => i.ToString(CultureInfo.InvariantCulture)
                ),
                data => data.FtlOrderBy(i => (byte)i).ThenBy(i => (sbyte)i)
                .ThenBy(i => (short)i).ThenBy(i => (ushort)i)
                .ThenBy(i => i).ThenBy(i => (uint)i)
                .ThenBy(i => (float)i).ThenBy(i => (double)i)
                .ThenBy(i => (decimal)i)
                .ThenBy(i => i % 2 == 0)
                .ThenBy(i => i.ToString(CultureInfo.InvariantCulture))
                );
        }

        [TestMethod]
        public void GetMethodLike_FindArraySortMethod()
        {
            var mi = typeof (Array).GetMethodLike(
                "Sort", Generic.FunArg(0).MakeArrayType(), Generic.FunArg(1).MakeArrayType(),
                typeof (int), typeof (int), typeof (IComparer<>).MakeGenericType(Generic.FunArg(0)));

            Assert.IsNotNull(mi);

            var methodInstance = mi.MakeGenericMethod(typeof (int), typeof (string));
            var parameters = methodInstance.GetParameters();
            Assert.AreSame(typeof(int[]), parameters[0].ParameterType);
            Assert.AreSame(typeof(string[]), parameters[1].ParameterType);
            Assert.AreSame(typeof(IComparer<int>), parameters[4].ParameterType);
        }

        [TestMethod]
        public void GetMethodLike_FindListSortMethod()
        {
            var mi = typeof(List<>).GetMethodLike(
                "Sort", typeof(IComparer<>).MakeGenericType(Generic.Arg(0)));

            Assert.IsNotNull(mi);
            var parameters = mi.GetParameters();
            Assert.AreSame(typeof(IComparer<>), parameters[0].ParameterType.GetGenericTypeDefinition());
        }

        [TestMethod]
        public void IsIdentityTransform_ReturnParameter_IsIdentity()
        {
            Expression<Func<int, int>> f = i => i;
            Assert.IsTrue(f.IsIdentityTransform());
        }

        [TestMethod]
        public void IsIdentityTransform_ReturnUnrelatedConstant_NotIdentity()
        {
            Expression<Func<int, int>> f = i => -5;
            Assert.IsFalse(f.IsIdentityTransform());
        }

        [TestMethod]
        public void IsIdentityTransform_ReturnRelatedTransform_NotIdentity()
        {
            Expression<Func<int, int>> f2 = i => i + 1;
            Assert.IsFalse(f2.IsIdentityTransform());
        }

        [TestMethod]
        public void IsIdentityTransform_ReturnDifferentType_NotIdentity()
        {
            Expression<Func<int, long>> f3 = i => i;
            Assert.IsFalse(f3.IsIdentityTransform());
        }

        [TestMethod]
        public void IsIdentityTransform_NotUnivariate_NotIdentity()
        {
            Expression<Func<int, string, int>> f4 = (i, s) => i;
            Assert.IsFalse(f4.IsIdentityTransform());
        }
    }
}
