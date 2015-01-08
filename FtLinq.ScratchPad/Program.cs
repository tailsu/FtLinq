using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using GameRayBurst.FtLinq;
using System.Linq;
using GameRayBurst.FtLinq.Recompiler;
using Logos.Utility;

namespace FtLinq.ScratchPad
{

    internal struct Foo
    {
        public int a;
        public int b;

    }

    class Program
    {
        static void Main()
        {
            //Debugger.Break();
            var rng = new Random(1885);
            var a = new MyKey() { A = rng.Next(), B = rng.Next() };
            var b = new MyKey() { A = rng.Next(), B = rng.Next() };
            var arr = new[] { a, b };

            Array.Sort(arr);

            //Console.WriteLine(a.CompareTo(b));

            var data = FtlEnumerable.Generate(() => rng.Next(), 8000000).ToArray();

            var sw = new Stopwatch();
            IEnumerable<int> sorted, baseline, baselineComposite, baselineKeyed;

            //sw.Restart();
            //sorted = data.OrderBy(_ => _).ToList();
            //sw.Stop();
            //Console.WriteLine("LINQ: {0:N2}s", sw.Elapsed.TotalSeconds);
            //baseline = sorted;

            //sw.Restart();
            //sorted = data.OrderBy(_ => _, new MyComparer()).ToList();
            //sw.Stop();
            //Console.WriteLine("LINQ with comparer: {0:N2}s", sw.Elapsed.TotalSeconds);
            //CheckSeq(baseline, sorted);

            //sw.Restart();
            //sorted = data.OrderBy(i => -i).ToList();
            //sw.Stop();
            //Console.WriteLine("LINQ with non-trivial selector: {0:N2}s", sw.Elapsed.TotalSeconds);
            //baselineKeyed = sorted;

            //sw.Restart();
            //var copy = data.ToArray();
            //Array.Sort(copy);
            //sw.Stop();
            //Console.WriteLine("Builtin: {0:N2}s", sw.Elapsed.TotalSeconds);
            //CheckSeq(baseline, copy);

            //sw.Restart();
            //var keys = data.Select(i => new MyKey { A = i % 10, B = i / 10 }).ToArray();
            //copy = data.ToArray();
            //Array.Sort(keys, copy);
            //sw.Stop();
            //Console.WriteLine("Builtin with composite key: {0:N2}s", sw.Elapsed.TotalSeconds);
            //baselineComposite = copy;

            //sw.Restart();
            //keys = data.Select(i => new MyKey { A = i % 10, B = i / 10 }).ToArray();
            //copy = data.ToArray();
            //Array.Sort(keys, copy, new MyKeyComparer());
            //sw.Stop();
            //Console.WriteLine("Builtin with composite key and explicit comparer: {0:N2}s", sw.Elapsed.TotalSeconds);
            //CheckSeq(baselineComposite, copy);

            //sw.Restart();
            //sorted = data.FtlSort(Comparer<int>.Default).ToList();
            //sw.Stop();
            //Console.WriteLine("Fast and simple: {0:N2}s", sw.Elapsed.TotalSeconds);
            //CheckSeq(baseline, sorted);

            //sw.Restart();
            //sorted = data.FtlOrderBy(i => -i, Comparer<int>.Default).ToList();
            //sw.Stop();
            //Console.WriteLine("Fast with selector: {0:N2}s", sw.Elapsed.TotalSeconds);
            //CheckSeq(baselineKeyed, sorted);

            //sw.Restart();
            //sorted = data.FtlSort(new MyComparer()).ToList();
            //sw.Stop();
            //Console.WriteLine("Fast with comparer: {0:N2}s", sw.Elapsed.TotalSeconds);
            //CheckSeq(baseline, sorted);

            //sw.Restart();
            //sorted = data.FtlOrderBy(i => -i, new MyComparer()).ToList();
            //sw.Stop();
            //Console.WriteLine("Fast with selector and comparer: {0:N2}s", sw.Elapsed.TotalSeconds);
            //CheckSeq(baselineKeyed, sorted);

            sw.Restart();
            sorted = data.FtlOrderBy(i => new MyKey { A = i % 10000, B = i / 10 }).ToList();
            sw.Stop();
            Console.WriteLine("Fast with composite key selector: {0:N2}s", sw.Elapsed.TotalSeconds);
            //CheckSeq(baselineComposite, sorted);

            //sw.Restart();
            //sorted = data.FtlOrderBy(i => new MyKey { A = i % 10, B = i / 10 }, new MyKeyComparer()).ToList();
            //sw.Stop();
            //Console.WriteLine("Fast with composite key selector and comparer: {0:N2}s", sw.Elapsed.TotalSeconds);
            //CheckSeq(baselineComposite, sorted);

            //sw.Restart();
            //sorted = data.OrderBy(i => i % 10).ThenBy(i => i / 10).ToList();
            //sw.Stop();
            //Console.WriteLine("LINQ OrderBy.ThenBy: {0:N2}s", sw.Elapsed.TotalSeconds);
            //CheckSeq(baselineComposite, sorted);

            sw.Restart();
            sorted = data.FtlOrderBy(i => i % 10000).ThenBy(i => i / 10).ToList();
            sw.Stop();
            Console.WriteLine("Fast OrderBy.ThenBy: {0:N2}s", sw.Elapsed.TotalSeconds);
            //CheckSeq(baselineComposite, sorted);
        }

        private static void CheckSeq(IEnumerable<int> baseline, IEnumerable<int> sorted)
        {
            if (!baseline.SequenceEqual(sorted))
                throw new ApplicationException();
        }
    }
   
    struct MyKey : IComparable<MyKey>
    {
        public int A;
        public int B;
        
        public int CompareTo(MyKey other)
        {
//            Debugger.Break();
            if (A != other.A)
                return A - other.A;
            return B - other.B;
        }
    }

    class MyKeyComparer : IComparer<MyKey>
    {
        public int Compare(MyKey x, MyKey y)
        {
            var a = x.A - y.A;
            if (a != 0)
                return a;
            var b = x.B - y.B;
            if (b != 0)
                return b;
            return 0;
        }
    }

    class MyComparer : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            return x - y;
        }
    }
}
