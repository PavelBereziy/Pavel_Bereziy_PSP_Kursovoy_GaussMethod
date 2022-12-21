using System;
using System.IO;
using System.Text.RegularExpressions;

namespace GausApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int n = -1;
            double[][] a = null;
            double[] x = null;
            double[] b = null;
            string pathA = "a2.txt",
                   pathB = "b2.txt",
                   pathX = "x2.txt";
            bool isParall = false;
            string ip = "127.0.0.1";
            int countClient = 2;
            // проверяем параметры строки
            if (args.Length > 0)
            {
                // если есть используем их
                pathA = args[0];
                pathB = args[1];
                pathX = args[2];
                if (args[3].Equals("p"))
                {
                    isParall = true;
                    ip = args[4];
                    countClient = int.Parse(args[5]);
                }
            }
           
            // чтение файла с A матрицей
            using (StreamReader sr = new StreamReader(pathA, System.Text.Encoding.Default))
            {
                string line;
                string[] str;
                int i = 0, j = 0;
                while ((line = sr.ReadLine()) != null)
                {

                    line = Regex.Replace(line, @"\s+", " ").Trim();
                    str = line.Split(' ');
                    if (n == -1)
                    {
                        n = str.Length;
                        a = new double[n][];
                        for (int l = 0; l < n; l++)
                        {
                            a[l] = new double[n + 1];
                        }
                        b = new double[n];
                        x = new double[n];
                    }
                    for (j = 0; j < n; j++)
                    {
                        a[i][j] = double.Parse(str[j]);
                    }
                    i++;
                }
            }
            // чтение файла с B матрицей
            using (StreamReader sr = new StreamReader(pathB, System.Text.Encoding.Default))
            {
                string line;
                string[] str;
                int i = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    line = Regex.Replace(line, @"\s+", " ").Trim();
                    a[i][n] = double.Parse(line);
                    b[i] = double.Parse(line);
                    i++;
                }
            }
            // запуск вычисления
            GausMethod gaus = new GausMethod();
            if (isParall)
            {
                // Распределенное
                x = gaus.GaussParalel(a, b, n, ip, countClient);
            } else
            {
                // Прямое решение
                x = gaus.Gauss(a, b, n);
            }
            
            // вывод x в файл
            using (StreamWriter sw = new StreamWriter(pathX, false, System.Text.Encoding.Default))
            {
                string str;
                for (int i = 0; i < n; i++)
                {
                    str = "     ";
                    if (Math.Round(x[i] / 10000, 7, MidpointRounding.ToEven) == 0)
                        str += "0.0000000";
                    else
                        str += Math.Round(x[i] / 10000, 7, MidpointRounding.ToEven).ToString("0.#######");
                    sw.WriteLine(str);
                }
            }

        }
    }
}
