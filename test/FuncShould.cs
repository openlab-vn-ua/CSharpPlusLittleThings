using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenLab.Plus.UnitTest
{
    using OpenLab.Plus.Func;

    [TestClass]
    public class FuncShould
    {
        [TestMethod]
        public void BeCreatedAndExecuted()
        {
            var F0 = FuncCall.Create(() => { return 1; });
            var F1 = FuncCall.Create((a) => { return a + 1; }, 1);
            var F2 = FuncCall.Create((a, b) => { return a + b + 1; }, 1, 2);
            var F3 = FuncCall.Create((a, b, c) => { return a + b + c+ 1; }, 1, 2, 3);

            // First call

            Assert.AreEqual(1, F0.Call().Result);
            Assert.AreEqual(2, F1.Call().Result);
            Assert.AreEqual(4, F2.Call().Result);
            Assert.AreEqual(7, F3.Call().Result);

            // Second call (not sure we have to support this, but why not)

            Assert.AreEqual(1, F0.Call().Result);
            Assert.AreEqual(2, F1.Call().Result);
            Assert.AreEqual(4, F2.Call().Result);
            Assert.AreEqual(7, F3.Call().Result);
        }

        [TestMethod]
        public void BeCreatedAndExecutedWithDifferentParams()
        {
            var F0 = FuncCall.Create(() => { return "!"; });
            var F1 = FuncCall.Create((a) => { return a + 1; }, "!");
            var F2 = FuncCall.Create((a, b) => { return a + b + 1; }, 1, "!!");
            var F3 = FuncCall.Create((a, b, c) => { return a + b + c + 1; }, 1, "!!", "###");

            // First call

            Assert.AreEqual("!", F0.Call().Result);
            Assert.AreEqual("!1", F1.Call().Result);
            Assert.AreEqual("1!!1", F2.Call().Result);
            Assert.AreEqual("1!!###1", F3.Call().Result);

            // Second call (not sure we have to support this, but why not)

            Assert.AreEqual("!", F0.Call().Result);
            Assert.AreEqual("!1", F1.Call().Result);
            Assert.AreEqual("1!!1", F2.Call().Result);
            Assert.AreEqual("1!!###1", F3.Call().Result);
        }
    }
}
