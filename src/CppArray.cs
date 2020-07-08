namespace OpenLab.Plus.MarshalPlus
{
    /// <summary>
    /// C/C++ Fixed marshal helper
    /// </summary>
    /// <remarks>Open source software with MIT license</remarks>

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Generic data access interface
    /// </summary>
    public interface ICppArray
    {
        /// <summary> Return number of elements in array </summary>
        int Length { get; }

        /// <summary> Return pointer to array (in unmanaged memory) </summary>
        IntPtr ArrayDataPtr { get; }

        /// <summary> Return data size of array in bytes </summary>
        int ArrayDataSize { get; }

        /// <summary> Return pointer to element (in unmanaged memory) </summary>
        IntPtr GetValuePtr(int Index);

        /// <summary> Return data size of (marshaled) element in bytes </summary>
        int ValueDataSize { get; }
    }

    public interface ICppArray<TValue> : ICppArray
    {
        /// <summary> Allocate array like malloc(Count * sizeof(TValue)) and optionally fill it with zeros [low level operation]</summary>
        void AllocRawSpace(int Count, bool ZeroMemory);

        /// <summary> Allocate array and fill it by default values. If TValue is reference type, null values lead to zero target memory </summary>
        void Alloc(int Count);

        /// <summary> Allocate array by copy from specified Source </summary>
        void AllocFrom(IEnumerable<TValue> Source);

        /// <summary> Free allocated array </summary>
        void Free();

        /// <summary> Return array element (reconstructed from its marshaled copy in memory) </summary>
        TValue GetValue(int Index);

        /// <summary> Set array element (marshaled data will be filled to memory). if TValue is reference type and null, memory will be filled with zeros </summary>
        void SetValue(TValue Item, int Index);

        /// <summary> Indexer to allow regular access with [] operator</summary>
        TValue this[int i] { get; set; }
    }

    public class CppArray<TValue> : ICppArray<TValue>, IDisposable, IEnumerable<TValue>
    {
        #region Internal

        protected static readonly IntPtr NULL = IntPtr.Zero;

        protected IntPtr        TheArrayDataPtr = NULL;
        protected int           TheArrayLength = 0;
        protected readonly int  TheValueDataSize = Marshal.SizeOf<TValue>(); // cache
        protected readonly bool TheValueIsValueType = typeof(TValue).IsValueType; // cache
        protected bool[]        TheArrayValueIsSet = null; // Will be TheArrayLength size, contains bool in case item is set

        protected object SyncRoot { get { return this; } }

        #endregion

        #region Basic

        public int Length { get { return TheArrayLength; } }

        public IntPtr ArrayDataPtr { get { return TheArrayDataPtr; } }
        public int ArrayDataSize { get { return TheValueDataSize * TheArrayLength; } }

        public IntPtr GetValuePtr(int Index)
        {
            if ((Index < 0) || (Index >= TheArrayLength))
            {
                throw new IndexOutOfRangeException();
            }

            if ((TheArrayDataPtr == NULL) || (TheArrayLength <= 0) || (TheValueDataSize <= 0))
            {
                throw new InvalidOperationException();
            }

            return (TheArrayDataPtr + (TheValueDataSize * Index));
        }

        public int ValueDataSize { get { return TheValueDataSize; } }

        #endregion

        #region Typed

        public void ClearMemory(IntPtr Ptr, int Size)
        {
            byte[] Filler = new byte[128]; // This will be initialized by zeros, use it as filler

            int Step;
            int Offset = 0;

            while(Offset < Size)
            {
                Step = Filler.Length;

                if ((Offset + Step) > Size)
                {
                    Step = Size - Offset;
                }

                Marshal.Copy(Filler, 0, Ptr, Step);
                Offset += Step;
            }
        }

        public void AllocRawSpace(int Count, bool ZeroMemory)
        {
            Free();

            if (Count > 0)
            {
                try
                {
                    lock (SyncRoot)
                    {
                        TheArrayDataPtr = Marshal.AllocHGlobal(TheValueDataSize * Count);
                        TheArrayLength = Count;
                        TheArrayValueIsSet = new bool[Count]; // will be init as array of false
                        if (ZeroMemory) { ClearMemory(TheArrayDataPtr, TheValueDataSize * Count); }
                    }
                }
                catch
                {
                    Free();
                    throw;
                }
            }
        }

        public void Alloc(int Count)
        {
            if (TheValueIsValueType)
            {
                AllocRawSpace(Count, false);
                for (int i = 0; i < Count; i++)
                {
                    SetValue(default(TValue), i);
                }
            }
            else
            {
                AllocRawSpace(Count, true);
            }
        }

        public void AllocFrom(IEnumerable<TValue> Source)
        {
            var SourceCount = Source.Count();

            AllocRawSpace(SourceCount, false);
            int i = 0;

            foreach(var Item in Source)
            {
                if (i >= SourceCount)
                {
                    // Source is changed (and extended while we are working)
                    Free(); 
                    throw new InvalidOperationException("Collection size was modified (too long now)"); 
                }
                SetValue(Item, i);
                i++;
            }

            if (i < SourceCount)
            {
                // Source is changed (and shortened while we are working)
                Free(); 
                throw new InvalidOperationException("Collection size was modified (too short now)");
            }
        }

        public void Free()
        {
            if (TheArrayDataPtr == NULL) { return; }

            lock (SyncRoot)
            {
                try
                {
                    if (TheArrayDataPtr != NULL)
                    {
                        try
                        {
                            for (int i = 0; i < TheArrayLength; i++)
                            {
                                if (TheArrayValueIsSet[i])
                                {
                                    DeleteValue(i);
                                }
                            }
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(TheArrayDataPtr);
                        }
                    }
                }
                finally
                {
                    TheArrayDataPtr = NULL;
                    TheArrayLength = 0;
                    TheArrayValueIsSet = null;
                }
            }
        }

        public TValue GetValue(int Index)
        {
            if (TheValueIsValueType)
            {
                if (!TheArrayValueIsSet[Index])
                {
                    throw new InvalidOperationException($"Value with index {Index} was not set before get (for TValue is value type)");
                }
            }
            else
            {
                if (!TheArrayValueIsSet[Index])
                {
                    return default(TValue); // return null;
                }
            }

            return Marshal.PtrToStructure<TValue>(GetValuePtr(Index));
        }

        public void SetValue(TValue Item, int Index)
        {
            if (!TheValueIsValueType)
            {
                if ((object)Item == null) // null
                {
                    DeleteValue(Index);
                    return;
                }
            }

            var wasSet = TheArrayValueIsSet[Index];
            Marshal.StructureToPtr<TValue>(Item, GetValuePtr(Index), wasSet);
            TheArrayValueIsSet[Index] = true;
        }

        public TValue this[int i] { get { return GetValue(i); } set { SetValue(value, i); } }

        // Extra functions

        public bool IsValueSet(int Index)
        {
            return TheArrayValueIsSet[Index];
        }

        public void DeleteValue(int Index, bool ZeroMemory)
        {
            if (TheArrayValueIsSet[Index])
            {
                Marshal.DestroyStructure<TValue>(GetValuePtr(Index));
                TheArrayValueIsSet[Index] = false;
                if (ZeroMemory) { ClearMemory(GetValuePtr(Index), TheValueDataSize); }
            }
        }

        public void DeleteValue(int Index)
        {
            DeleteValue(Index, true);
        }

        public bool IsAllValuesSet()
        {
            return !TheArrayValueIsSet.Any((c) => c == false);
        }

        #endregion

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            Free();
        }

        ~CppArray()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Enumerable

        protected sealed class EnumOnList<T> : IEnumerator<T>
        {
            private ICppArray<T> TheHost;
            private int ThePosition = -1;

            public EnumOnList(ICppArray<T> Host)
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
                return (ThePosition < TheHost.Length);
            }

            public void Reset()
            {
                ThePosition = -1;
            }
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return new EnumOnList<TValue>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new EnumOnList<TValue>(this);
        }

        #endregion

        #region ctors

        public CppArray(int Count)
        {
            Alloc(Count);
        }

        public CppArray(IEnumerable<TValue> Source)
        {
            AllocFrom(Source);
        }

        #endregion
    }
}
