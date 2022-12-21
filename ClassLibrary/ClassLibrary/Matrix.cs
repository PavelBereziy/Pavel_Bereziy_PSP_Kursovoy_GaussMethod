using System;

namespace ClassLibrary
{
    [Serializable]
    public class Matrix
    {
        public string Message { get; set; }
        public int Index { get; set; }
        public int K { get; set; }
        public int N { get; set; }
        public double[] ArrayI { get; set; }

        public double[][] Array { get; set; }
        public Matrix() { }
        public Matrix(string message, int index, int n, int k, double[] arrayI, double[][] array)
        {
            N = n;
            K = k;
            Message = message;
            Index = index;
            ArrayI = arrayI;
            Array = array;
        }
    }
}
