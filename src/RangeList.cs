namespace OpenLab.Plus.Collections.Generic
{
    /// <summary>
    /// RangeList Extension for Colletions
    /// </summary>
    /// <remarks>Open source software with MIT license</remarks>

    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// ReadOnlyRangeList Numerical Range that acts as a IList&lt;<typeparamref name="T"/>&gt; (where T is any integer type).
    /// Similar to Enumerable.Range(int start, int count), but also exposes IList interfaces (read only).
    /// To construct instance of this class use EnumerablePlus.RangeAsList(start, count).
    /// To construct pure int instance of this class use EnumerablePlus.RangeList(int start, int count).
    /// </summary>
    /// <typeparam name="T">Integral type (sbyte,byte,short,ushort,int,uint,long,ulong)</typeparam>
    public class ReadOnlyRangeList<T> : IList<T> where T : IComparable
    {
        protected T TheStart;
        protected int TheCount;

        internal IValueHandler TheHandler;

        /// <summary>
        /// Should implement value operations need for list
        /// Should have checked statement inside every method to detect overflow
        /// </summary>
        internal interface IValueHandler
        {
            // 
            int CastToInt(T a);
            T AddInt(T a, int b);
            T Add(T a, T b);
            int DiffAsInt(T a, T b);
        }

        internal ReadOnlyRangeList(T start, int count, IValueHandler Handler)
        {
            if (count < 0) { throw new ArgumentOutOfRangeException(); }

            if (count > 0)
            {
                try 
                { 
                    Handler.AddInt(start, count - 1); 
                } 
                catch(Exception e) 
                { 
                    throw new ArgumentOutOfRangeException();
                }
            }

            TheStart = start;
            TheCount = count;
            TheHandler = Handler;
        }

        public T this[int index]
        {
            get
            {
                if (index < 0) { throw new ArgumentOutOfRangeException(); }
                if (index >= TheCount) { throw new ArgumentOutOfRangeException(); }
                return TheHandler.AddInt(TheStart, index);
            }

            set { throw new NotSupportedException(); }
        }

        public int Count { get { return TheCount; } }

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
            if (TheCount <= 0) { return false; }

            if (item.CompareTo(TheStart) < 0)
            {
                return (false);
            }

            var TheLast = TheHandler.AddInt(TheStart, Count - 1);

            if (item.CompareTo(TheLast) > 0)
            {
                return (false);
            }

            return (true);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            var Item = TheStart;

            for (int i = 0; i < TheCount; i++)
            {
                array[arrayIndex] = Item;

                if ((i + 1) < TheCount)
                {
                    Item = TheHandler.AddInt(Item, 1);
                }

                arrayIndex++;
            }
        }

        protected sealed class EnumOnList : IEnumerator<T>
        {
            private IList<T> TheHost;
            private int ThePosition = -1;

            public EnumOnList(IList<T> Host)
            {
                TheHost = Host;
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public T Current
            {
                get
                {
                    try
                    {
                        return TheHost[ThePosition];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }

            public void Dispose()
            {
                // Nothing to do
            }

            public bool MoveNext()
            {
                ThePosition++;
                return (ThePosition < TheHost.Count);
            }

            public void Reset()
            {
                ThePosition = -1;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new EnumOnList(this);
        }

        public int IndexOf(T item)
        {
            int NOT_FOUND = -1;

            if (TheCount <= 0) { return NOT_FOUND; }

            int StartCmp = item.CompareTo(TheStart);

            if (StartCmp < 0)
            {
                return (NOT_FOUND); // lower then start
            }
            else if (StartCmp == 0)
            {
                return 0; // Equal to start this is the first
            }

            var TheLast = TheHandler.AddInt(TheStart, TheCount - 1);

            int LastCmp = item.CompareTo(TheLast);

            if (LastCmp > 0)
            {
                return (NOT_FOUND); // upper then start
            }
            else if (LastCmp == 0)
            {
                return TheCount - 1; // This is last one
            }

            var Result = TheHandler.DiffAsInt(item, TheStart);

            if (Result < 0) { return NOT_FOUND; } // bug trap
            if (Result >= TheCount) { return NOT_FOUND; } // bug trap

            return (Result);
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
            return new EnumOnList(this);
        }
    }

    public partial class EnumerablePlus
    {
        protected class ByteValueHandler : ReadOnlyRangeList<Byte>.IValueHandler
        {
            public int CastToInt(Byte a) { return checked((int)a); }
            public Byte AddInt(Byte a, int b) { return checked((Byte)(a + b)); }
            public Byte Add(Byte a, Byte b) { return checked((Byte)(a + b)); }
            public int DiffAsInt(Byte a, Byte b) { return checked((int)(a - b)); }

            public static readonly ByteValueHandler Instance = new ByteValueHandler();
        }
        public static ReadOnlyRangeList<Byte> RangeAsList(Byte start, int count) => new ReadOnlyRangeList<Byte>(start, count, new ByteValueHandler());
        public static ReadOnlyRangeList<Byte> RangeAsList(Byte count) => new ReadOnlyRangeList<Byte>(0, new ByteValueHandler().CastToInt(count), new ByteValueHandler());

        protected class SByteValueHandler : ReadOnlyRangeList<SByte>.IValueHandler
        {
            public int CastToInt(SByte a) { return checked((int)a); }
            public SByte AddInt(SByte a, int b) { return checked((SByte)(a + b)); }
            public SByte Add(SByte a, SByte b) { return checked((SByte)(a + b)); }
            public int DiffAsInt(SByte a, SByte b) { return checked((int)(a - b)); }

            public static readonly SByteValueHandler Instance = new SByteValueHandler();
        }
        public static ReadOnlyRangeList<SByte> RangeAsList(SByte start, int count) => new ReadOnlyRangeList<SByte>(start, count, new SByteValueHandler());
        public static ReadOnlyRangeList<SByte> RangeAsList(SByte count) => new ReadOnlyRangeList<SByte>(0, new SByteValueHandler().CastToInt(count), new SByteValueHandler());

        protected class Int16ValueHandler : ReadOnlyRangeList<Int16>.IValueHandler
        {
            public int CastToInt(Int16 a) { return checked((int)a); }
            public Int16 AddInt(Int16 a, int b) { return checked((Int16)(a + (Int16)(b))); }
            public Int16 Add(Int16 a, Int16 b) { return checked((Int16)(a + b)); }
            public int DiffAsInt(Int16 a, Int16 b) { return checked((int)(a - b)); }

            public static readonly Int16ValueHandler Instance = new Int16ValueHandler();
        }
        public static ReadOnlyRangeList<Int16> RangeAsList(Int16 start, int count) => new ReadOnlyRangeList<Int16>(start, count, new Int16ValueHandler());
        public static ReadOnlyRangeList<Int16> RangeAsList(Int16 count) => new ReadOnlyRangeList<Int16>(0, new Int16ValueHandler().CastToInt(count), new Int16ValueHandler());

        protected class UInt16ValueHandler : ReadOnlyRangeList<UInt16>.IValueHandler
        {
            public int CastToInt(UInt16 a) { return checked((int)a); }
            public UInt16 AddInt(UInt16 a, int b) { return checked((UInt16)(a + (UInt16)(b))); }
            public UInt16 Add(UInt16 a, UInt16 b) { return checked((UInt16)(a + b)); }
            public int DiffAsInt(UInt16 a, UInt16 b) { return checked((int)(a - b)); }

            public static readonly UInt16ValueHandler Instance = new UInt16ValueHandler();
        }
        public static ReadOnlyRangeList<UInt16> RangeAsList(UInt16 start, int count) => new ReadOnlyRangeList<UInt16>(start, count, new UInt16ValueHandler());
        public static ReadOnlyRangeList<UInt16> RangeAsList(UInt16 count) => new ReadOnlyRangeList<UInt16>(0, new UInt16ValueHandler().CastToInt(count), new UInt16ValueHandler());

        protected class Int32ValueHandler : ReadOnlyRangeList<Int32>.IValueHandler
        {
            public int CastToInt(Int32 a) { return checked((int)a); }
            public Int32 AddInt(Int32 a, int b) { return checked((Int32)(a + (Int32)(b))); }
            public Int32 Add(Int32 a, Int32 b) { return checked((Int32)(a + b)); }
            public int DiffAsInt(Int32 a, Int32 b) { return checked((int)(a - b)); }

            public static readonly Int32ValueHandler Instance = new Int32ValueHandler();
        }
        public static ReadOnlyRangeList<Int32> RangeAsList(Int32 start, int count) => new ReadOnlyRangeList<Int32>(start, count, new Int32ValueHandler());
        public static ReadOnlyRangeList<Int32> RangeAsList(Int32 count) => new ReadOnlyRangeList<Int32>(0, new Int32ValueHandler().CastToInt(count), new Int32ValueHandler());

        protected class UInt32ValueHandler : ReadOnlyRangeList<UInt32>.IValueHandler
        {
            public int CastToInt(UInt32 a) { return checked((int)a); }
            public UInt32 AddInt(UInt32 a, int b) { return checked((UInt32)(a + (UInt32)(b))); }
            public UInt32 Add(UInt32 a, UInt32 b) { return checked((UInt32)(a + b)); }
            public int DiffAsInt(UInt32 a, UInt32 b) { return checked((int)(a - b)); }

            public static readonly UInt32ValueHandler Instance = new UInt32ValueHandler();
        }
        public static ReadOnlyRangeList<UInt32> RangeAsList(UInt32 start, int count) => new ReadOnlyRangeList<UInt32>(start, count, new UInt32ValueHandler());
        public static ReadOnlyRangeList<UInt32> RangeAsList(UInt32 count) => new ReadOnlyRangeList<UInt32>(0, new UInt32ValueHandler().CastToInt(count), new UInt32ValueHandler());

        protected class Int64ValueHandler : ReadOnlyRangeList<Int64>.IValueHandler
        {
            public int CastToInt(Int64 a) { return checked((int)a); }
            public Int64 AddInt(Int64 a, int b) { return checked((Int64)(a + (Int64)(b))); }
            public Int64 Add(Int64 a, Int64 b) { return checked((Int64)(a + b)); }
            public int DiffAsInt(Int64 a, Int64 b) { return checked((int)(a - b)); }

            public static readonly Int64ValueHandler Instance = new Int64ValueHandler();
        }
        public static ReadOnlyRangeList<Int64> RangeAsList(Int64 start, int count) => new ReadOnlyRangeList<Int64>(start, count, new Int64ValueHandler());
        public static ReadOnlyRangeList<Int64> RangeAsList(Int64 count) => new ReadOnlyRangeList<Int64>(0, new Int64ValueHandler().CastToInt(count), new Int64ValueHandler());

        protected class UInt64ValueHandler : ReadOnlyRangeList<UInt64>.IValueHandler
        {
            public int CastToInt(UInt64 a) { return checked((int)a); }
            public UInt64 AddInt(UInt64 a, int b) { return checked((UInt64)(a + (UInt64)(b))); }
            public UInt64 Add(UInt64 a, UInt64 b) { return checked((UInt64)(a + b)); }
            public int DiffAsInt(UInt64 a, UInt64 b) { return checked((int)(a - b)); }

            public static readonly UInt64ValueHandler Instance = new UInt64ValueHandler();
        }
        public static ReadOnlyRangeList<UInt64> RangeAsList(UInt64 start, int count) => new ReadOnlyRangeList<UInt64>(start, count, new UInt64ValueHandler());
        public static ReadOnlyRangeList<UInt64> RangeAsList(UInt64 count) => new ReadOnlyRangeList<UInt64>(0, new UInt64ValueHandler().CastToInt(count), new UInt64ValueHandler());

        /// <summary>
        /// Pure ReadOnly List of int (Similar to Enumerable.Range).
        /// This method is drop-in replacement for Enumerable.Range, but returned IList&lt;int&gt;
        /// </summary>
        /// <param name="start">start value to count from</param>
        /// <param name="count">total values in list</param>
        /// <returns></returns>
        public static ReadOnlyRangeList<int> RangeList(int start, int count) => RangeAsList(start, count);
    }
}
