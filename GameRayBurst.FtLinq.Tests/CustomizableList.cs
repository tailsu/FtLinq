using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests
{
    class CustomizableList<T> : IList<T>
    {
        public virtual IEnumerator<T> GetEnumerator()
        {
            throw new AssertFailedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void Add(T item)
        {
            throw new AssertFailedException();
        }

        public virtual void Clear()
        {
            throw new AssertFailedException();
        }

        public virtual bool Contains(T item)
        {
            throw new AssertFailedException();
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            throw new AssertFailedException();
        }

        public virtual bool Remove(T item)
        {
            throw new AssertFailedException();
        }

        public virtual int Count
        {
            get { throw new AssertFailedException(); }
        }

        public virtual bool IsReadOnly
        {
            get { throw new AssertFailedException(); }
        }

        public virtual int IndexOf(T item)
        {
            throw new AssertFailedException();
        }

        public virtual void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public virtual void RemoveAt(int index)
        {
            throw new AssertFailedException();
        }

        public virtual T this[int index]
        {
            get { throw new AssertFailedException(); }
            set { throw new AssertFailedException(); }
        }
    }
}
