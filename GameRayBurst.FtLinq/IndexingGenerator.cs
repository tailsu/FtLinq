using System;
using System.Collections;
using System.Collections.Generic;

namespace GameRayBurst.FtLinq
{
    public struct IndexingGenerator<T> : IEnumerable<T>, IEnumerator<T>
    {
        public readonly Func<int, T> GeneratorFunction;
        public readonly int Count;

        private int current;

        public IndexingGenerator(Func<int, T> generator, int count)
            : this()
        {
            GeneratorFunction = generator;
            Count = count;
            Reset();
        }

        public IndexingGenerator<T> GetEnumerator()
        {
            return new IndexingGenerator<T>(GeneratorFunction, Count);
        }

        public bool MoveNext()
        {
            current++;
            return current < Count;
        }

        public void Reset()
        {
            current = -1;
        }

        public T Current
        {
            get { return GeneratorFunction(current); }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}