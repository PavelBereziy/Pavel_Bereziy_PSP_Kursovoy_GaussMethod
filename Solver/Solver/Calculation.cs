using System;
using System.Collections.Generic;
using System.Net.Sockets;
using ClassLibrary;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.Threading;

namespace Solver
{
    public class Calculation
    {
        TcpClient client = null;
        NetworkStream stream = null;
        private StreamReader reader;
        private StreamWriter writer;
        Matrix matrix = null;
        bool isEnd = false;
        double[][] arr;
        int n, k;
        List<int> counts = new List<int>();
        string name;

        /// <summary>
        /// подключение клиента
        /// </summary>
        /// <param name="clientIndex">номер</param>
        /// <param name="address">ip</param>
        /// <param name="port">порт</param>
        public void connect(int clientIndex, string address, int port)
        {
            Console.WriteLine("Connect...");
            name = "Client #" + clientIndex + " ";
            // подключение клиента
            client = new TcpClient(address, port);
            Console.WriteLine(name + "conected");
            stream = client.GetStream();
            // отправка сообщения о том что подключился успешно
            Matrix mtrx = new Matrix(name, port, -1, -1, null, null);
            writer = new StreamWriter(stream, Encoding.UTF8);
            writeData(mtrx);
            // получение данных о размерах матрицы и количетве пакетов
            reader = new StreamReader(stream, Encoding.UTF8);
            matrix = readData();
            n = matrix.N;
            k = matrix.K;
            arr = new double[k][];
            for (int i = 0; i < k; i++)
            {
                arr[i] = new double[n];
            }
            getData();
        }
        /// <summary>
        /// Получение всех пакетов для расчетов
        /// </summary>
        public void getData()
        {
            Console.WriteLine("GetData...");
            int count = 0;
            do
            {
                // Десериализасия пакета
                matrix = readData();
                // пока не -1 читаем пакеты
                if (matrix.Index != -1)
                {
                    Console.WriteLine("Client read part{0}", matrix.Index);
                    arr[count] = matrix.ArrayI;
                    count++;
                    counts.Add(matrix.Index);
                }
                // -1 чтение закончено можно приступать к расчетам
                else
                {
                    Console.WriteLine("Client read all data...");
                    break;
                }
            } while (true);
            calculation();
        }
        /// <summary>
        /// расчеты данных пакетов
        /// </summary>
        public void calculation()
        {
            Console.WriteLine("Calculation...");
            bool isRepid = false;
            int count = 0;
            double buff;
            stream = client.GetStream();
            do
            {
                try
                {
                    matrix = readData();
                    count = matrix.Index;
                    if (count == -1)
                    {
                        setData();
                        isEnd = true;
                    } else
                    // расчет пакетов исходных данных
                    if (counts.IndexOf(count) >= 0 && !isRepid)
                    {
                        Console.WriteLine(name + " has part {0}", count);
                        int ind = counts.IndexOf(count);
                        buff = arr[ind][count];
                        Console.WriteLine(name + " calculate part {0}", count);
                        for (int j = ind; j < n; j++)
                        {
                            arr[ind][j] = Math.Round(arr[ind][j] / buff, 5);
                        }
                        // отпрвка промежуточных рачетов
                        matrix = new Matrix(name, matrix.Index, -1, -1, arr[ind], null);
                        writeData(matrix);
                        isRepid = true;
                    }
                    else
                    {
                        // расчет пакетов с использованием данных полученных от других клиентов
                        if (matrix.ArrayI != null)
                        {

                            Console.WriteLine(name + " calculate  all parts with part {0}", count);
                            int l = 0, i = 0, j = 0;
                            while (l < k && counts[l] <= count) l++;
                            try
                            {
                                // расчет своих пакетов с пакетом полученным от других клиентов
                                for (i = l; i < k; i++)
                                {
                                    buff = arr[i][count];
                                    for (j = count + 1; j < n; j++)
                                    {
                                        arr[i][j] = Math.Round(arr[i][j] - (matrix.ArrayI[j] * buff), 5);
                                    }
                                }
                            }
                            catch(Exception e)
                            {
                               
                            }
                            // отпрвка промежуточных рачетов
                            matrix = new Matrix(name + " p", -2, -1, -1, null, null);
                            writeData(matrix);
                            isRepid = false;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            } while (!isEnd);
        }
        /// <summary>
        /// Отвправка конечных результатов
        /// </summary>
        public void setData()
        {
            Console.WriteLine("SetData...");
           
            Console.WriteLine(name + " return all parts");
            // отправка всех расчитанных пакетов и
            // Сообщение об окончании отпрвки
            double[] countArr = new double[counts.Count];
            for (int i=0;i<countArr.Length;i++)
            {
                countArr[i] = Convert.ToDouble(counts[i]);
            }
            matrix = new Matrix(name, -3, -1, -1, countArr, arr);
            writeData(matrix);
            while (true)
            {
                // Получаем данные если есть стрим
                while (stream.DataAvailable)
                {
                    matrix = readData();
                }
                // Получаем сообщение о завершении и завершаем работу
                if (matrix.Index == -1)
                {
                    break;
                }                

            }
            client.Close();

        }
        /// <summary>
        /// метод передачи данных серверу
        /// </summary>
        /// <param name="matrix">объект данных</param>
        public void writeData(Matrix matrix)
        {
            string jdata = convertWriteData(matrix);
            writer.WriteLine(jdata);
            writer.Flush();
        }
        /// <summary>
        /// метод перевода данных из объекта в строку для передачи по сети
        /// </summary>
        /// <returns>объект данных</returns>
        public string convertWriteData(Matrix matrix)
        {
            return JsonConvert.SerializeObject(matrix);
        }
        /// <summary>
        /// метод получения данных от сервера
        /// </summary>
        /// <returns>объект данных</returns>
        public Matrix readData()
        {
            string json;
            json = reader.ReadLine();
            while (json != "" && json != null && json[0] != '{')
            {
                json = json.Remove(0, 1);
            }
            return convertReadData(json);
        }
        /// <summary>
        /// метод перевода данных из строки в объект для работы с ним
        /// </summary>
        /// <returns>строка данных</returns>
        public Matrix convertReadData(string json)
        {
            Matrix mat = new Matrix();
            try
            {
                mat = JsonConvert.DeserializeObject<Matrix>(json);
            }
            catch (Exception e)
            {

            }
            return mat;
        }
    }
}
