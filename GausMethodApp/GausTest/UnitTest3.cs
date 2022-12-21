using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Solver;
using ClassLibrary;

namespace GausTest
{
    [TestClass]
    public class UnitTest3
    {
        private Calculation calc;

        public UnitTest3()
        {
            calc = new Calculation();
        }

        [TestMethod]
        public void TestMethod1()
        {
            string expected = "{\"Message\":\"\",\"Index\":0,\"K\":10,\"N\":10,\"ArrayI\":null,\"Array\":null}";
            Matrix mat = new Matrix("", 0, 10, 10, null, null);
            string result = calc.convertWriteData(mat);
            Assert.AreEqual(expected, result, "Write data corrected");
        }

        [TestMethod]
        public void TestMethod2()
        {
            string expected = "{\"Message\":\"\",\"Index\":0,\"K\":-1,\"N\":-1,\"ArrayI\":[0.0,1.0,2.0,0.1,3.0,0.0,1.0,2.0,0.1,3.0],\"Array\":null}";
            Matrix mat = new Matrix("", 0, -1, -1, new double[10] { 0.0, 1.0, 2.0, 0.1, 3.0, 0.0, 1.0, 2.0, 0.1, 3.0 }, null);
            string result = calc.convertWriteData(mat);
            Assert.AreEqual(expected, result, "Write data corrected");
        }

        [TestMethod]
        public void TestMethod3()
        {
            string expected = "{\"Message\":\"Client #0\",\"Index\":8080,\"K\":-1,\"N\":-1,\"ArrayI\":null,\"Array\":null}";
            Matrix mat = new Matrix("Client #0", 8080, -1, -1, null, null);
            string result = calc.convertWriteData(mat);
            Assert.AreEqual(expected, result, "Write data corrected");
        }

        [TestMethod]
        public void TestMethod4()
        {
            string json = "{\"Message\":\"Client #0\",\"Index\":8080,\"K\":-1,\"N\":-1,\"ArrayI\":null,\"Array\":null}";
            Matrix mat = calc.convertReadData(json);
            string expected = "Client #0";
            Assert.AreEqual(expected, mat.Message, "Read data corrected");
        }

        [TestMethod]
        public void TestMethod5()
        {
            string json = "{\"Message\":\"\",\"Index\":0,\"K\":-1,\"N\":-1,\"ArrayI\":[0.0,1.0,2.0,0.1,3.0,0.0,1.0,2.0,0.1,3.0],\"Array\":null}";
            Matrix mat = calc.convertReadData(json);
            double[] expected = new double[10] { 0.0, 1.0, 2.0, 0.1, 3.0, 0.0, 1.0, 2.0, 0.1, 3.0 };
            CollectionAssert.AreEqual(expected, mat.ArrayI, "Read data corrected");
        }
    }
}
