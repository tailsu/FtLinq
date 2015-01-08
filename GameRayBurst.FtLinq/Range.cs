using System.Collections;
using System.Collections.Generic;

namespace GameRayBurst.FtLinq
{
    public struct Range : IEnumerable<int>, IEnumerator<int>
    {
        public readonly int Start;
        public readonly int Count;

        private int current;

        public Range(int start, int count) : this()
        {
            Start = start;
            Count = count;
            Reset();
        }

        public bool MoveNext()
        {
            current++;
            return current < Start + Count;
        }

        public void Reset()
        {
            current = Start - 1;
        }

        public int Current
        {
            get { return current; }
        }

        public void Dispose()
        {
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public Range GetEnumerator()
        {
            return new Range(Start, Count);
        }

        IEnumerator<int> IEnumerable<int>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}