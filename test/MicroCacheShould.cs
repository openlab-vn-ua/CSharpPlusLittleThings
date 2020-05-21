using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenLab.Plus.UnitTest
{
    using System.Threading;
    using OpenLab.Plus.Caching;
    using OpenLab.Plus.Func;

    [TestClass]
    public class MicroCacheShould
    {
        [TestMethod]
        public void BeCreated()
        {
            var MyCache = new MicroCache();
        }

        [TestMethod]
        public void StoreData()
        {
            var MyCache = new MicroCache();

            int Maker0CallCount = 0;
            Func<int> Maker0 = () =>
            {
                Maker0CallCount++;
                return 0;
            };

            Assert.AreEqual(0, MyCache.GetOrMake(Maker0));
            Assert.AreEqual(0, MyCache.GetOrMake(Maker0));
            Assert.AreEqual(0, MyCache.GetOrMake(Maker0));
            Assert.AreEqual(0, MyCache.GetOrMake(Maker0));
            Assert.AreEqual(1, Maker0CallCount);

            int Maker1CallCount = 0;
            Func<int, long> Maker1 = (a) =>
            {
                Maker1CallCount++;
                return a + 1;
            };

            // Same maker, 2 differnt set of arguments (should be 2 calls), 3 calls per set
            Assert.AreEqual(5, MyCache.GetOrMake(Maker1, 4));
            Assert.AreEqual(5, MyCache.GetOrMake(Maker1, 4));
            Assert.AreEqual(5, MyCache.GetOrMake(Maker1, 4));
            Assert.AreEqual(6, MyCache.GetOrMake(Maker1, 5));
            Assert.AreEqual(6, MyCache.GetOrMake(Maker1, 5));
            Assert.AreEqual(6, MyCache.GetOrMake(Maker1, 5));
            Assert.AreEqual(2, Maker1CallCount);

            MyCache.DefCacheItemAbsoluteExpirationTicksCount = 200;
            Thread.Sleep(100);
            // (100 < 200) should not expiry yet
            Assert.AreEqual(5, MyCache.GetOrMake(Maker1, 4));
            Assert.AreEqual(6, MyCache.GetOrMake(Maker1, 5));
            Assert.AreEqual(2, Maker1CallCount);

            Thread.Sleep(150);
            // 100+150 > 200 (should now expiry yet)
            Assert.AreEqual(5, MyCache.GetOrMake(Maker1, 4));
            Assert.AreEqual(6, MyCache.GetOrMake(Maker1, 5));
            Assert.AreEqual(2+2, Maker1CallCount);
        }

        protected int Func2X(int a) { return a * 2; }
        protected int Func3X(int a) { return a * 3; }

        [TestMethod]
        public void DistinguishDifferentMethods()
        {
            var MyCache = new MicroCache();

            Assert.AreEqual(10, MyCache.GetOrMake(Func2X, 5));
            Assert.AreEqual(15, MyCache.GetOrMake(Func3X, 5));
        }

        protected int Call2XWithClosure(MicroCache MyCache, int value, int offset)
        {
            return MyCache.GetOrMake((int a) => { return a * 2 + offset; }, value);
        }

        [TestMethod]
        public void CacheClosuresIgnoringGeneratedThis()
        {
            var MyCache = new MicroCache();

            // Maker's 'this' is not a part of key.by default (as it will be diffrent for each clousre isntance)

            Assert.AreEqual(11, Call2XWithClosure(MyCache, 5, 1));
            Assert.AreEqual(11, Call2XWithClosure(MyCache, 5, 2)); // will be 12 if Closure called (but it should not)
        }

        protected class Sample
        {
            int offset;
            public Sample(int offset) { this.offset = offset; }
            public int Func2X(int a) { return a * 2 + offset; }
        }

        [TestMethod]
        public void IgnoreDifferentThisToSupportClosures()
        {
            var MyCache = new MicroCache();

            var Obj1 = new Sample(1);
            var Obj2 = new Sample(2);

            // Maker's 'this' is not a part of key.by default (as it will be diffrent for each clousre isntance)

            Assert.AreEqual(11, MyCache.GetOrMake(Obj1.Func2X, 5));
            Assert.AreEqual(11, MyCache.GetOrMake(Obj2.Func2X, 5)); // will be 12 if Obj2 called (but it should not)
        }
    }
}
