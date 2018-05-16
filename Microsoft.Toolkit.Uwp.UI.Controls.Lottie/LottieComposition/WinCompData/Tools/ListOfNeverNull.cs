using System;
using System.Collections;
using System.Collections.Generic;

namespace WinCompData.Tools
{
#if !WINDOWS_UWP
    public
#endif
    sealed class ListOfNeverNull<T> : IList<T>
    {
        readonly List<T> _wrapped = new List<T>();

        public T this[int index]
        {
            get => _wrapped[index];

            set => _wrapped[index] = AssertNotNull(value);
        }

        public int Count => _wrapped.Count;

        public bool IsReadOnly => ((IList<T>)_wrapped).IsReadOnly;

        public void Add(T item)
        {
            _wrapped.Add(AssertNotNull(item));
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public void Clear()
        {
            _wrapped.Clear();
        }

        public bool Contains(T item)
        {
            return _wrapped.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _wrapped.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IList<T>)_wrapped).GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return _wrapped.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _wrapped.Insert(index, AssertNotNull(item));
        }

        public bool Remove(T item)
        {
            return _wrapped.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _wrapped.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<T>)_wrapped).GetEnumerator();
        }

        static T AssertNotNull(T item)
        {
            if (item == null)
            {
                throw new ArgumentException();
            }
            return item;
        }

    }
}
