using System.Collections;
using System.Collections.Generic;

namespace GameRayBurst.FtLinq
{
    public struct Repetition<T> : IEnumerable<T>, IEnumerator<T>
    {
        public readonly T Value;
        public readonly int Count;
        public int current;

        public Repetition(T value, int count) : this()
        {
            Value = value;
            Count = count;
            Reset();
        }

        public Repetition<T> GetEnumerator()
        {
            return new Repetition<T>(Value, Count);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
            get { return Value; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public void Dispose()
        {
        }
    }
}