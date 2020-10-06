using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenLab.Plus.UnitTest
{
    using OpenLab.Plus.SystemPlus;

    [TestClass]
    public class ObjectClonerShould
    {
        class APair<T>
        {
            public T First;
            public T Second { get; set; }
        }

        protected void AssertAreEquals(dynamic expected, dynamic actual)
        {
            Assert.AreEqual(expected.First, actual.First);
            Assert.AreEqual(expected.Second, actual.Second);
        }

        protected Random RNG = new Random();

        protected int RV()
        {
            return RNG.Next(1, 1000);
        }

        protected string RS()
        {
            return "S"+RV().ToString();
        }

        [TestMethod]
        public void CloneObjects()
        {
            //var a = new { First = 101, Second = 102 };
            //var b = new { First = 201L, Second = 202L };

            if (true) { var A = new APair<int> { First = RV(), Second = RV() }; AssertAreEquals(A, ObjectCloner.CreateMemberwiseClone(A)); }
            if (true) { var A = new APair<long> { First = RV(), Second = RV() }; AssertAreEquals(A, ObjectCloner.CreateMemberwiseClone(A)); }
            if (true) { var A = new APair<float> { First = RV(), Second = RV() }; AssertAreEquals(A, ObjectCloner.CreateMemberwiseClone(A)); }
            if (true) { var A = new APair<double> { First = RV(), Second = RV() }; AssertAreEquals(A, ObjectCloner.CreateMemberwiseClone(A)); }
            if (true) { var A = new APair<decimal> { First = RV(), Second = RV() }; AssertAreEquals(A, ObjectCloner.CreateMemberwiseClone(A)); }
            if (true) { var A = new APair<string> { First = RS(), Second = RS() }; AssertAreEquals(A, ObjectCloner.CreateMemberwiseClone(A)); }
        }

        [TestMethod]
        public void CopyObjectsWithFieldsOfAssignableTypes()
        {
            var A = new APair<int>() { First = RV(), Second = RV() };
            var B = new APair<long>();
            ObjectCloner.MemberwiseCopy(A, B);
            AssertAreEquals(A, B);
        }

        [TestMethod]
        public void CopyFromAnonimousObjectsWithFieldsOfSameTypes()
        {
            var A = new { First = (int)RV(), Second = (int)RV() };
            var B = new APair<int>();
            ObjectCloner.MemberwiseCopy(A, B);
            AssertAreEquals(A, B);
        }

        [TestMethod]
        public void CopyFromAnonimousObjectsWithFieldsOfAssignableTypes()
        {
            var A = new { First = (int)RV(), Second = (short)RV() };
            var B = new APair<long>();
            ObjectCloner.MemberwiseCopy(A, B);
            AssertAreEquals(A, B);
        }

        [TestMethod]
        public void FailToCopyFieldsOfUnassignableTypes()
        {
            var A = new { First = (int)10, Second = (long)20 };
            var B = new APair<byte>();
            Assert.ThrowsException<ArgumentException>(() =>
            {
                ObjectCloner.MemberwiseCopy(A, B);
            });
        }
    }
}
