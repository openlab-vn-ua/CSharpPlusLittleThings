namespace OpenLab.Plus.Collections.Generic
{
    /// <summary>
    /// EmptyList Extension for Colletions
    /// </summary>
    /// <remarks>Open source software with MIT license</remarks>

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// ReadOnlyEmptyList as empty IList&lt;<typeparamref name="T"/>&gt; (where T is any type).
    /// Similar to Enumerable.Empty&lt;<typeparamref name="T"/>&gt;, but also exposses IList interfaces (read only).
    /// To construct instance of this class use EnumerablePlus.EmptyList&lt;<typeparamref name="T"/>&gt;().
    /// </summary>
    /// <typeparam name="T">Any type to be contained in a list</typeparam>
    public class ReadOnlyEmptyList<T> : IList<T>
    {
        public T this[int index]
        {
            get { throw new ArgumentOutOfRangeException(); }
            set { throw new NotSupportedException(); }
        }

        public int Count { get { return 0; } }

        public bool IsReadOnly { get { return true; } }

        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(T item)
        {
            return (false);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            // Nothing to copy
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerable.Empty<T>().GetEnumerator();
        }

        public int IndexOf(T item)
        {
            int NOT_FOUND = -1;
            return NOT_FOUND;
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Enumerable.Empty<T>().GetEnumerator();
        }
    }

    public partial class EnumerablePlus
    {
        /// <summary>
        /// Pure Empty List of T (Similar to Enumerable.Empty).
        /// This method is drop-in replacement for Enumerable.Empty&lt;T&gt;, but returned IList&lt;T&gt;
        /// </summary>
        /// <returns>Empty read only list of &lt;T&gt;</returns>
        /// <typeparam name="T">List inner type</typeparam>
        public static ReadOnlyEmptyList<T> EmptyList<T>()
        {
            return new ReadOnlyEmptyList<T>();
        }
    }
}
