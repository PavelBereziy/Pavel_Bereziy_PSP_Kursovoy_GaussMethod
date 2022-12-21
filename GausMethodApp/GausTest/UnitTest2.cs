using GausApp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GausTest
{
    [TestClass]
    public class UnitTest2
    {
        private GausMethod gaus;

        public UnitTest2()
        {
            gaus = new GausMethod();
        }

        [TestMethod]
        public void TestMethod1()
        {
            int n = 2;
            double[] data = { 5, 2, 2, 1 };
            double[][] a = new double[n][];
            int k = 0;
            for (int i=0;i<n;i++)
            {
                a[i] = new double[n];
                for (int j=0;j<n;j++)
                {
                    a[i][j] = data[k];
                    k++;
                }
            }
            double[] b = { 7, 9 };
            double[] x = { -11, 31 };
            double[] xr = gaus.Gauss(a, b, n);
            CollectionAssert.AreEqual(x, RoundArray(xr), "Result is correct");
        }

        [TestMethod]
        public void TestMethod2()
        {
            int n = 3;
            double[] data = { 2, 3, -1, 1, -2, 1, 1, 0, 2 };
            double[][] a = new double[n][];
            int k = 0;
            for (int i = 0; i < n; i++)
            {
                a[i] = new double[n];
                for (int j = 0; j < n; j++)
                {
                    a[i][j] = data[k];
                    k++;
                }
            }
            double[] b = { 9, 3, 2 };
            double[] x = { 4, 0, -1 };
            double[] xr = gaus.Gauss(a, b, n);
            CollectionAssert.AreEqual(x, RoundArray(xr), "Result is correct");
        }

        [TestMethod]
        public void TestMethod3()
        {
            int n = 4;
            double[] data = { 1, -1, 3, 1, 4, -1, 5, 4, 2, -2, 4, 1, 1, - 4, 5, -1 };
            double[][] a = new double[n][];
            int k = 0;
            for (int i = 0; i < n; i++)
            {
                a[i] = new double[n];
                for (int j = 0; j < n; j++)
                {
                    a[i][j] = data[k];
                    k++;
                }
            }
            double[] b = { 5, 4, 6, 3 };
            double[] x = { 9, 18, 10, -16 };
            double[] xr = gaus.Gauss(a, b, n);
            CollectionAssert.AreEqual(x, RoundArray(xr), "Result is correct");
        }

        private double[] RoundArray(double[] array)
        {
            for (int i = 0; i < array.Length; i++) 
            {
                array[i] = Math.Round(array[i], 0);
            }
            return array;
        }
    }
}
