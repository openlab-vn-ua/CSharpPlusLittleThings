using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenLab.Plus.UnitTest
{
    using System.Linq;
    using OpenLab.Plus.Collections.Generic;
    using static OpenLab.Plus.Collections.Generic.EnumerablePlus;

    [TestClass]
    public class EmptyListShould
    {
        static void VerifyItemTypeAtComple<T>(IList<T> List, T Item = default(T)) { } // Compile time check

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

        static void VerifyEmpty<T>(IList<T> L)
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => L[0]);
            Assert.AreEqual(false, L.Contains(default(T)));
            Assert.AreEqual(0, L.Count);
            Assert.AreEqual(-1, L.IndexOf(default(T)));
        }

        static void VerifyEmptyList<T>()
        {
            var L = EmptyList<T>();
            VerifyItemTypeAtComple<T>(L);
            VerifyReadOnly(L);
            VerifyEmpty(L);
        }

        [TestMethod]
        public void BeCreated()
        {
            VerifyEmptyList<sbyte>();
            VerifyEmptyList<bool>();
            VerifyEmptyList<int>();
            VerifyEmptyList<double>();
            VerifyEmptyList<float>();
            VerifyEmptyList<decimal>();
            VerifyEmptyList<object>();
            VerifyEmptyList<string>();
            VerifyEmptyList<System.IO.DriveType>(); // enum
            VerifyEmptyList<System.IO.FileAttributes>(); // enum
            VerifyEmptyList<System.IO.ErrorEventHandler>(); // delegate
            VerifyEmptyList<System.IO.WaitForChangedResult>(); // struct
            VerifyEmptyList<EmptyListShould>(); // user class
        }
    }
}
