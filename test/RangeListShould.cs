using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenLab.Plus.UnitTest
{
    using System.Linq;
    using OpenLab.Plus.Collections.Generic;
    using static OpenLab.Plus.Collections.Generic.EnumerablePlus;

    [TestClass]
    public class RangeListShould
    {
        static void PrintList<T>(IList<T> Source)
        {
            Console.WriteLine($"List of {typeof(T)} with {Source.Count} elements:");
            for (int i = 0; i < Source.Count; i++)
            {
                Console.WriteLine($"List[{i}]={Source[i]}");
            }
        }

        static T GetLast<T>(IList<T> Source)
        {
            if (Source.Count <= 0) { throw new InvalidOperationException(); }
            return (Source[Source.Count - 1]);
        }

        static T GetFirst<T>(IList<T> Source)
        {
            if (Source.Count <= 0) { throw new InvalidOperationException(); }
            return (Source[0]);
        }

        static void VerifyItemTypeAtComple<T>(IList<T> List, T Item = default(T)) {  } // Compile time check

        static void VerifyList<T>(IList<T> L, T start, int count, T last)
        {
            Assert.AreEqual(count, L.Count);
            if (count > 0)
            {
                Assert.AreEqual(start, GetFirst(L), "First element should be start");
                Assert.AreEqual(last, GetLast(L), "First element should be last");
                // fast access
                Assert.IsTrue(L.Contains(start), "Should contain start element");
                Assert.IsTrue(L.Contains(last), "Should contain last element");
                Assert.AreEqual(0, L.IndexOf(start), "Should find index of start element");
                Assert.AreEqual(count - 1, L.IndexOf(last), "Should find index of last element");
            }
        }

        static void VerifyLinq<T>(IList<T> L, T start, int count, T last)
        {
            Assert.AreEqual(count, L.Count);
            if (count > 0)
            {
                Assert.AreEqual(start, L.Min());
                Assert.AreEqual(last, L.Max());
                Assert.AreEqual(count, L.Count());
            }
        }

        static void VerifyReadOnly<T>(IList<T> L)
        {
            Assert.ThrowsException<NotSupportedException>(() => L.Clear());
            Assert.ThrowsException<NotSupportedException>(() => L.Add(default(T)));
            Assert.ThrowsException<NotSupportedException>(() => L.Remove(default(T)));

            if (L.Count > 0)
            {
                Assert.ThrowsException<NotSupportedException>(() => L.Insert(0, default(T)));
                Assert.ThrowsException<NotSupportedException>(() => L.RemoveAt(0));
            }
        }

        static void VerifyFoundInsider<T>(IList<T> L, T insider, int expectedInsiderIndex = -1)
        {
            Assert.IsTrue(L.Contains(insider), "Should contain insider element");
            var insiderIndex = L.IndexOf(insider);

            if (expectedInsiderIndex >= 0)
            {
                Assert.AreEqual(expectedInsiderIndex, insiderIndex, "Wrong insider index");
            }

            Assert.IsTrue(insiderIndex >= 0, "Insider should be found");
            Assert.IsTrue(insiderIndex < L.Count, "Insider should be found");
            Assert.IsTrue(L[insiderIndex].Equals(insider), "Insider should be on its place");

            // Linq

            T insiderLinq;

            insiderLinq = L.First((s) => s.Equals(insider));
            Assert.AreEqual(insider, insiderLinq, "Wrong (first) insider found by Linq");

            insiderLinq = L.Last((s) => s.Equals(insider));
            Assert.AreEqual(insider, insiderLinq, "Wrong (last) insider found by Linq");
        }

        static void VerifyNotFoundOutsider<T>(IList<T> L, T ousider)
        {
            Assert.IsFalse(L.Contains(ousider), "Should not contain outsider element");
            Assert.IsTrue(L.IndexOf(ousider) < 0, "Ouisider element should not be found");
        }

        static long GetValue<T>(T item)
        {
            { if (item is SByte itemValue) { return itemValue; } }
            { if (item is Byte itemValue) { return itemValue; } }
            { if (item is Int16 itemValue) { return itemValue; } }
            { if (item is UInt16 itemValue) { return itemValue; } }
            { if (item is Int32 itemValue) { return itemValue; } }
            { if (item is UInt32 itemValue) { return itemValue; } }
            { if (item is Int64 itemValue) { return itemValue; } }
            { if (item is UInt64 itemValue) { throw new Exception("UnitTests. GetValue: ulong not supported (supported to any type except ulong"); } }
            throw new Exception($"UnitTests. GetValue: cannot detect type for item {item}");
        }

        static void VerifyData<T>(IList<T> L, T start, int count, bool verifyFind = false)
        {
            Assert.AreEqual(count, L.Count);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => L[-1]);

            if (L.Count <= 0)
            {
                Assert.ThrowsException<ArgumentOutOfRangeException>(() => L[0]);
                Assert.ThrowsException<ArgumentOutOfRangeException>(() => L[1]);
            }
            else
            {
                Assert.ThrowsException<ArgumentOutOfRangeException>(() => L[L.Count]);

                if (typeof(T) == typeof(ulong))
                {
                    // Skip: cannot verify content of ulong at the moment
                }
                else
                {
                    // Verify content

                    var s = L[0];
                    var e = L[L.Count - 1];

                    int i = 0;
                    foreach (var item in L)
                    {
                        var expValue = GetValue(start) + i;
                        Assert.AreEqual(item, L[i]);
                        Assert.AreEqual(expValue, GetValue(item));

                        if (verifyFind) { VerifyFoundInsider(L, item); }
                        i++;
                    }

                    for (i = 0; i < count; i++)
                    {
                        var item = L[i];
                        var expValue = GetValue(start) + i;
                        Assert.AreEqual(item, L[i]);
                        Assert.AreEqual(expValue, GetValue(item));
                    }
                }
            }
        }

        static void VerifyAll<T>(IList<T> L)
        {
            VerifyReadOnly(L);
        }

        static void VerifyAll<T>(IList<T> L, T start, int count, T last)
        {
            VerifyReadOnly(L);
            VerifyList(L, start, count, last);

            if (count < 1000)
            {
                VerifyLinq(L, start, count, last);
            }

            if (count < 1000)
            {
                VerifyData(L, start, count, true);
            }
            else if (count < 10000)
            {
                VerifyData(L, start, count);
            }
            else
            {
                // Do not verify data, too much work
            }

            if (count < 1000)
            {
                var Copy = L.ToArray().ToList();
                VerifyData(Copy, start, count, true);
                //CollectionAssert.AreEqual(Copy, L);
                Assert.IsTrue(Copy.SequenceEqual(L));
            }
        }

        [TestMethod]
        public void BeCreatedWithAllSupportedTypes()
        {
            { var L = RangeAsList((sbyte)-1, 10); VerifyItemTypeAtComple<sbyte>(L); VerifyReadOnly(L); }
            { var L = RangeAsList((byte)1, 10); VerifyItemTypeAtComple<byte>(L); VerifyReadOnly(L); }

            { var L = RangeAsList((ushort)1, 10); VerifyItemTypeAtComple<ushort>(L); VerifyReadOnly(L); }
            { var L = RangeAsList((short)-1, 10); VerifyItemTypeAtComple<short>(L); VerifyReadOnly(L); }

            { var L = RangeAsList((uint)1, 10); VerifyItemTypeAtComple<uint>(L); VerifyReadOnly(L); }
            { var L = RangeAsList((int)-1, 10); VerifyItemTypeAtComple<int>(L); VerifyReadOnly(L); }

            { var L = RangeAsList((ulong)1, 10); VerifyItemTypeAtComple<ulong>(L); VerifyReadOnly(L); }
            { var L = RangeAsList((long)-1, 10); VerifyItemTypeAtComple<long>(L); VerifyReadOnly(L); }
        }

        [TestMethod]
        public void BeCreatedWithTypeInferenceFromValue()
        {
            // int as default
            { var L = RangeAsList(0, 10); VerifyItemTypeAtComple<int>(L); VerifyReadOnly(L); }
            { var L = RangeAsList(2147483647, 0); VerifyItemTypeAtComple<int>(L); VerifyReadOnly(L); }
            { var L = RangeAsList(2147483648, 0); VerifyItemTypeAtComple<uint>(L); VerifyReadOnly(L); }
        }

        [TestMethod]
        public void BeCreatedWithTypeInferenceFromVar()
        {

            { sbyte x = 1; var L = RangeAsList(x, 10); VerifyItemTypeAtComple<sbyte>(L); VerifyReadOnly(L); }
            { long  x = 1; var L = RangeAsList(x, 10); VerifyItemTypeAtComple<long>(L); VerifyReadOnly(L); }
            { ulong x = 1; var L = RangeAsList(x, 10); VerifyItemTypeAtComple<ulong>(L); VerifyReadOnly(L); }
        }

        [TestMethod]
        public void BeCreatedNoMatterOfCountType()
        {
            { ulong x = 1; var L = RangeAsList(x, (byte)(10)); VerifyItemTypeAtComple(L); VerifyReadOnly(L); }
            { ulong x = 1; var L = RangeAsList(x, (short)(10)); VerifyItemTypeAtComple(L); VerifyReadOnly(L); }
        }

        [TestMethod]
        public void BeCreatedWithCount0()
        {
            { var L = RangeAsList(0, 0); VerifyReadOnly(L); }
            { var L = RangeAsList(10, 0); VerifyReadOnly(L); }
        }

        [TestMethod]
        public void NotBeCreatedWithNegativeCount()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { var L = RangeAsList(0, -1); });
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { var L = RangeAsList(-1, -1); });
        }

        [TestMethod]
        public void BeCreatedWithSingleCountArgument()
        {
            { var L = RangeAsList((short)10); VerifyItemTypeAtComple<short>(L); VerifyAll<short>(L, 0, 10, 9); }
            { var L = RangeAsList(10); VerifyItemTypeAtComple<int>(L); VerifyAll<int>(L, 0, 10, 9); }
            { var L = RangeAsList(10L); VerifyItemTypeAtComple<long>(L); VerifyAll<long>(L, 0, 10, 9); }
            { var L = RangeAsList(10UL); VerifyItemTypeAtComple<ulong>(L); VerifyAll<ulong>(L, 0, 10, 9); }
        }

        [TestMethod]
        public void BeCreatedAsSimpleInt32RangeList()
        {
            var E = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            // List should integer only
            { var L = RangeList(1, 10); VerifyItemTypeAtComple<int>(L); VerifyAll<int>(L, 1, 10, 10); Assert.IsTrue(E.SequenceEqual(L)); }
            { var L = RangeList((short)1, 10); VerifyItemTypeAtComple<int>(L); VerifyAll<int>(L, 1, 10, 10); Assert.IsTrue(E.SequenceEqual(L)); }
            { var L = RangeList((sbyte)1, 10); VerifyItemTypeAtComple<int>(L); VerifyAll<int>(L, 1, 10, 10); Assert.IsTrue(E.SequenceEqual(L)); }
            { var L = RangeList((byte)1, 10); VerifyItemTypeAtComple<int>(L); VerifyAll<int>(L, 1, 10, 10); Assert.IsTrue(E.SequenceEqual(L)); }
        }

        [TestMethod]
        public void VerifyCreationRange()
        {
            { var L = RangeAsList((sbyte)0, 0); Assert.AreEqual(0, L.Count); VerifyAll(L); } // OK, zero length
            { var L = RangeAsList((sbyte)0, 1); Assert.AreEqual(1, L.Count); VerifyAll(L); } // OK, 1 length

            sbyte MAX = 127;
            sbyte MIN = -128;

            {
                // 0,MAX = 0..MAX-1
                var s = (sbyte)(0); int c = MAX; var e = (sbyte)(MAX - 1);
                var L = RangeAsList(s, c); VerifyAll(L, s, c, e);
            }

            {
                // 0,MAX+1 = 0..MAX
                var s = (sbyte)(0); int c = MAX + 1; var e = (sbyte)(MAX);
                var L = RangeAsList(s, c); VerifyAll(L, s, c, e);
            }

            {
                // -1,MAX+2 = -1..MAX
                var s = (sbyte)(-1); int c = MAX + 2; var e = (sbyte)(MAX);
                var L = RangeAsList(s, c); VerifyAll(L, s, c, e);
            }

            {
                // MIN,(-MIN) = MIN..-1
                var s = (sbyte)(MIN); int c = -MIN; var e = (sbyte)(-1);
                var L = RangeAsList(s, c); VerifyAll(L, s, c, e);
            }

            {
                // MIN,(-MIN+1) = MIN..0
                var s = (sbyte)(MIN); int c = -MIN + 1; var e = (sbyte)(0);
                var L = RangeAsList(s, c); VerifyAll(L, s, c, e);
            }

            {
                // MIN,MAX-MIN+1 = MIN..0..MAX
                var s = (sbyte)(MIN); int c = MAX - MIN + 1; var e = (sbyte)(MAX);
                var L = RangeAsList(s, c);
                Assert.AreEqual(0, L.IndexOf(s));
                Assert.AreEqual(1, L.IndexOf((sbyte)(s+1)));
                Assert.AreEqual(c - 1, L.IndexOf(e));
                Assert.AreEqual(c - 2, L.IndexOf((sbyte)(e-1)));
                VerifyAll(L, s, c, e);
            }

            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                // 0,MAX+2 = 0..MAX+1
                var L = RangeAsList((sbyte)0, MAX+2);
            });
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                // 1,MAX+1 = 1..MAX+1
                var L = RangeAsList((sbyte)1, MAX+1);
            });
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                // 2,MAX = 2..MAX+1
                var L = RangeAsList((sbyte)2, MAX);
            });
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                // 3,MAX-1 = 3..MAX+1
                var L = RangeAsList((sbyte)3, MAX-1);
            });
        }

        [TestMethod]
        public void SupportHugeValues()
        {
            { var L = RangeAsList(2147483648, 10); VerifyAll(L, 2147483648, 10, 2147483657); } // uint
            { var L = RangeAsList(4294967296, 10); VerifyAll(L, 4294967296, 10, 4294967305); } // long
        }
    }
}