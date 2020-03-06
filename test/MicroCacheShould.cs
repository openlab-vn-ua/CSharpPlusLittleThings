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
    }
}
