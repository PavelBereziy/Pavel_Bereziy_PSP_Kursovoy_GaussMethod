using ClassLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GausTest
{
    [TestClass]
    public class UnitTest1
    {
        private Matrix matrix;
        public UnitTest1()
        {
            matrix = new Matrix("Test 1", 1, 4, 2, new double[4] { 1, 4, 5, 7 }, null);
        }
        [TestMethod]
        public void TestMethod1()
        {
            Assert.AreEqual(1, matrix.Index, 0, "Index corrected");
        }
        [TestMethod]
        public void TestMethod2()
        {
            Assert.AreEqual(4, matrix.N, 0, "N corrected");
        }
        [TestMethod]
        public void TestMethod3()
        {
            Assert.AreEqual(4, matrix.ArrayI.Length, 0, "Array size corrected");
        }
        [TestMethod]
        public void TestMethod4()
        {
            Assert.AreEqual(5, matrix.ArrayI[2], 0, "Array data corrected");
        }
    }
}
