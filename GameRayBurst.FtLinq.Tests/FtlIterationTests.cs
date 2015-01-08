using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests
{
    [TestClass]
    public class FtlIterationTests
    {
        [TestMethod]
        public void ArrayIterator_Simple()
        {
            FtlAssert.Works((int[] source) => source.ToList());
        }

        [TestMethod]
        public void ListIterator_Simple()
        {
            FtlAssert.Works(
                (List<int> source) => source.ToList(),
                FtlAssert.TestData.ToList());
        }

        [TestMethod]
        public void EnumerableIterator_TypedInterface()
        {
            FtlAssert.Works(
                (IEnumerable<int> source) => source.ToList(),
                FtlAssert.TestData);
        }

        [TestMethod]
        public void EnumerableIterator_TypedConcrete()
        {
            FtlAssert.Works(
                (HashSet<int> source) => source.ToList(),
                new HashSet<int>(FtlAssert.TestData));
        }

        public class OpaqueEnumeratorList : List<int>
        {
            public OpaqueEnumeratorList(IEnumerable<int> enumerable)
                : base(enumerable)
            {}

            public new IEnumerator<int> GetEnumerator()
            {
                return base.GetEnumerator();
            }
        }

        [TestMethod]
        public void EnumerableIterator_TypedConcreteEnumerableAndInterfaceEnumerator()
        {
            FtlAssert.Works(
                (OpaqueEnumeratorList source) => source.ToList(),
                new OpaqueEnumeratorList(FtlAssert.TestData));
        }

        [TestMethod]
        public void EnumerableIterator_UntypedInterface()
        {
            FtlAssert.Works(
                (IEnumerable array) => array.Cast<int>().ToList(),
                new ArrayList(FtlAssert.TestData)
                );
        }

        [TestMethod]
        public void EnumerableIterator_UntypedConcrete()
        {
            FtlAssert.Works(
                (ArrayList array) => array.Cast<int>().ToList(),
                new ArrayList(FtlAssert.TestData)
                );
        }

        private class IndexableOnlyList : CustomizableList<int>
        {
            public override int this[int index]
            {
                get { return data[index]; }
                set { data[index] = value; }
            }

            public override int Count
            {
                get { return data.Length; }
            }

            private int[] data = FtlAssert.TestData;

            public override void CopyTo(int[] array, int arrayIndex)
            {
                Array.Copy(data, 0, array, arrayIndex, data.Length);
            }
        }

        [TestMethod]
        public void IListImplementationIterator_Simple()
        {
            FtlAssert.Works((IndexableOnlyList source) => source.ToList(), new IndexableOnlyList());
        }

        [TestMethod]
        public void IListInterfaceIterator_Simple()
        {
            FtlAssert.Works((IList<int> source) => source.ToList(), new IndexableOnlyList());
        }

        private class MyList : List<int>
        {
            public MyList(IEnumerable<int> source)
                : base(source)
            {}
        }

        [TestMethod]
        public void ListInherited_Simple()
        {
            FtlAssert.Works((MyList source) => source.ToList(), new MyList(FtlAssert.TestData));
        }

        [TestMethod]
        public void ObservableCollection_Simple()
        {
            FtlAssert.Works((ObservableCollection<int> source) => source.ToList(), new ObservableCollection<int>(FtlAssert.TestData));
        }

        private class SlicedList : CustomizableList<int>
        {
            public int MinIndex = 0;
            public int IndexCount = int.MaxValue;

            public override int this[int index]
            {
                get
                {
                    if (index < MinIndex || (IndexCount != int.MaxValue && index >= MinIndex + IndexCount))
                        Assert.Fail();

                    return index + 1;
                }
                set { }
            }

            public override int Count
            {
                get { return 100; }
            }
        }

        [TestMethod]
        public void List_Skip_IsOptimized()
        {
            var query = Ftl.Compile((IList<int> source) => source.Skip(10).ToList());
            var data = new SlicedList {MinIndex = 10};
            query(data);
        }

        [TestMethod]
        public void List_Take_IsOptimized()
        {
            var query = Ftl.Compile((IList<int> source) => source.Take(50).ToList());
            var data = new SlicedList { IndexCount = 50 };
            query(data);
        }

        [TestMethod]
        public void List_SkipTake_IsOptimized()
        {
            var query = Ftl.Compile((IList<int> source) => source.Skip(10).Take(50).ToList());
            var data = new SlicedList { MinIndex = 10, IndexCount = 50 };
            query(data);
        }

        [TestMethod]
        public void List_SkipOptimization_UnfulfilledSkipDoesntCrash()
        {
            var query = Ftl.Compile((IList<int> source) => source.Skip(1000).ToList());
            var data = new SlicedList { MinIndex = 1000 };
            query(data);
        }

        [TestMethod]
        public void List_TakeOptimization_UnfulfilledTakeDoesntCrash()
        {
            var query = Ftl.Compile((IList<int> source) => source.Take(1000).ToList());
            var data = new SlicedList { IndexCount = 100 };
            query(data);
        }

        private class DisposableEnumeratorEnumerable : IEnumerable<int>
        {
            public bool IsEnumeratorDisposed;

            public IEnumerator<int> GetEnumerator()
            {
                return new DisposableEnumerator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public class DisposableEnumerator : IEnumerator<int>
            {
                private readonly DisposableEnumeratorEnumerable myParent;

                public DisposableEnumerator(DisposableEnumeratorEnumerable parent)
                {
                    myParent = parent;
                }

                public void Dispose()
                {
                    myParent.IsEnumeratorDisposed = true;
                }

                public bool MoveNext()
                {
                    return false;
                }

                public void Reset()
                {
                    throw new NotImplementedException();
                }

                public int Current
                {
                    get { throw new NotImplementedException(); }
                }

                object IEnumerator.Current
                {
                    get { return Current; }
                }
            }
        }

        [TestMethod]
        public void IEnumerable_EnumeratorDisposed()
        {
            var referenceEnumerable = new DisposableEnumeratorEnumerable();
            foreach (var _ in referenceEnumerable) {}
            Assert.IsTrue(referenceEnumerable.IsEnumeratorDisposed);

            var enumerable = new DisposableEnumeratorEnumerable();
            var query = Ftl.Compile((IEnumerable<int> source) => source.ToList());
            query(enumerable);
            Assert.IsTrue(enumerable.IsEnumeratorDisposed);
        }

        [TestMethod]
        public void EnumerableRange_Simple()
        {
            FtlAssert.WorksScalar(() => Enumerable.Range(1, 10).Sum());
        }

        [TestMethod]
        public void EnumerableRepeat_Simple()
        {
            FtlAssert.WorksScalar(() => Enumerable.Repeat(10, 10).Sum());
        }

        [TestMethod]
        public void Enumerable_SourceComesFromFunctionCall()
        {
            FtlAssert.WorksScalar(() => Enumerable.Sum(Enumerable.Range(Math.Max(10, 20), 5)));
        }

        [TestMethod]
        public void Recompilation_NullSource()
        {
            FtlAssert.Throws((int[] source) => source.ToList(), null);
        }

        [TestMethod, PerformanceTest]
        public void PreallocateOutput_FromArray()
        {
            FtlAssert.Works((int[] source) => source.PreallocateOutputFromInputLength().ToList());
        }

        [TestMethod]
        public void PreallocateOutput_FromList()
        {
            FtlAssert.Works((List<int> source) => source.PreallocateOutputFromInputLength().ToList(), FtlAssert.TestData.ToList());
        }

        [TestMethod]
        public void PreallocateOutput_FromCollection()
        {
            FtlAssert.Works((ICollection<int> source) => source.PreallocateOutputFromInputLength().ToList(), FtlAssert.TestData.ToList());
        }

        [TestMethod]
        public void PreallocateOutput_FromEnumerable_Throws()
        {
            FtlAssert.Throws<FtlException>(
                () => FtlAssert.Works((IEnumerable<int> source) => source.PreallocateOutputFromInputLength().ToList(), FtlAssert.TestData));
        }

        private static bool myGetIntsOnlyOnceCalled;
        private static IEnumerable<int> GetIntsOnlyOnce()
        {
            if (myGetIntsOnlyOnceCalled)
                throw new ApplicationException("GetIntsOnlyOnce called multiple times");
            myGetIntsOnlyOnceCalled = true;
            return new[] {1, 2, 3};
        }

        [TestMethod]
        public void Iteration_SourceExpressionEvaluatedOnlyOnce()
        {
            var q = Ftl.Compile(() => GetIntsOnlyOnce().ToList());
            q();
        }

        [TestMethod]
        public void Iteration_Dictionary()
        {
            FtlAssert.Works((Dictionary<int, string> source) => source.ToArray(),
                new Dictionary<int, string> { {5, "aaa"}, {6, "bbb" }});
        }
    }
}
