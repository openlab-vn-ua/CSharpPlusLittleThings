using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenLab.Plus.UnitTest
{
    using System.Runtime.InteropServices;
    using OpenLab.Plus.MarshalPlus;

    [TestClass]
    public class CppArrayShould
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)] // w_char strings
        public class TestObject
        {
            public int TestValue;
            public string TestText;

            #region Internal

            private const int BIAS = 333;
            private const string TPFX = "TO:";

            internal static TestObject Make(int i)
            {
                var r = new TestObject();
                r.TestValue = BIAS + i;
                r.TestText = TPFX + i.ToString();
                return r;
            }

            internal void Check(int i)
            {
                Assert.AreEqual(BIAS + i, TestValue, $"Content of TestValue should match for i={i}");
                Assert.AreEqual(TPFX + i.ToString(), TestText, $"Content of TestText should match for i={i}");
            }

            #endregion
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)] // w_char strings
        public struct TestStruct
        {
            public int TestValue;
            public string TestText;

            #region Internal

            private const int BIAS = 555;
            private const string TPFX = "TS:";

            internal static TestStruct Make(int i)
            {
                var r = new TestStruct();
                r.TestValue = BIAS + i;
                r.TestText = TPFX + i.ToString();
                return r;
            }

            internal void Check(int i)
            {
                Assert.AreEqual(BIAS + i, TestValue, $"Content of TestValue should match for i={i}");
                Assert.AreEqual(TPFX + i.ToString(), TestText, $"Content of TestText should match for i={i}");
            }

            #endregion
        }

        protected void Verify<T>(CppArray<T> Arr, int ArrLength, Action<CppArray<T>, int> Fill, Action<CppArray<T>, int> Check)
        {
            Assert.AreEqual(ArrLength, Arr.Length);

            Assert.IsTrue(Arr.Length >= 0);

            for (var i = 0; i < Arr.Length; i++)
            {
                Fill(Arr, i);
                Check(Arr, i);
            }

            Assert.ThrowsException<IndexOutOfRangeException>(() =>
            {
                var VM1 = Arr[-1];
            });

            if (ArrLength <= 0)
            {
                Assert.ThrowsException<IndexOutOfRangeException>(() =>
                {
                    var V0 = Arr[0];
                });
            }
            else
            {
                // Should not throw an exception
                var V0 = Arr[0];
                var V1 = Arr[Arr.Length-1];

                Assert.ThrowsException<IndexOutOfRangeException>(() =>
                {
                    var VOUT = Arr[Arr.Length];
                });

                Assert.ThrowsException<IndexOutOfRangeException>(() =>
                {
                    var VOUT = Arr[Arr.Length+1];
                });
            }
        }

        [TestMethod]
        public void BeCreatedSizeEmpty()
        {
            int SIZE = 0;
            var P1 = new CppArray<byte>(SIZE);
            var P4 = new CppArray<int>(SIZE);
            var P8 = new CppArray<long>(SIZE);
            var O1 = new CppArray<TestObject>(SIZE);
            var S1 = new CppArray<TestStruct>(SIZE);
        }

        [TestMethod]
        public void BeCreatedSizeOne()
        {
            int SIZE = 1;
            var P1 = new CppArray<byte>(SIZE);
            var P4 = new CppArray<int>(SIZE);
            var P8 = new CppArray<long>(SIZE);
            var O1 = new CppArray<TestObject>(SIZE);
            var S1 = new CppArray<TestStruct>(SIZE);
        }

        protected void ValueFill(CppArray<byte> a, int i) => a[i] = (byte)i;
        protected void ValueFill(CppArray<int> a, int i) => a[i] = (int)i;
        protected void ValueFill(CppArray<long> a, int i) => a[i] = (long)i;
        protected void ValueCheck<T>(CppArray<T> a, int i) => Assert.AreEqual(i.ToString(), a[i].ToString());

        [TestMethod]
        public void BeCreatedSizeMany()
        {
            int SIZE = 5;
            var P1 = new CppArray<byte>(SIZE);
            var P4 = new CppArray<int>(SIZE);
            var P8 = new CppArray<long>(SIZE);
            var O1 = new CppArray<TestObject>(SIZE);
            var S1 = new CppArray<TestStruct>(SIZE);

            Verify(P1, SIZE, ValueFill, ValueCheck);
            Verify(P4, SIZE, ValueFill, ValueCheck);
            Verify(P8, SIZE, ValueFill, ValueCheck);

            Verify(S1, SIZE, (a, i) => a[i] = TestStruct.Make(i), (a, i) => a[i].Check(i));
            Verify(O1, SIZE, (a, i) => a[i] = TestObject.Make(i), (a, i) => a[i].Check(i));
        }
    }
}
