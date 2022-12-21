using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ClassLibrary;
using Newtonsoft.Json;

namespace GausApp
{
    public class GausMethod
    {
        /// <summary>
        /// Элемент для расчета времени вычислений
        /// </summary>
        private Stopwatch sw = new Stopwatch();
        TcpClient client = null;
        /// <summary>
        /// Форматтер для сериализации и делесиализации json
        /// </summary>
        private StreamReader reader;
        private StreamWriter writer;
        /// <summary>
        /// Стрим потока для получения и записи данных
        /// </summary>
        public NetworkStream stream = null;
        /// <summary>
        /// Элемент матрица который передается между приложениями
        /// </summary>
        Matrix matrix = null;       
        /// <summary>
        /// Очередь свободных клиентов
        /// </summary>
        private Queue<int> freeClient = new Queue<int>();
        /// <summary>
        /// матрица а
        /// </summary>
        private double[][] a;
        /// <summary>
        /// размер матрица
        /// </summary>
        private int n, key;
        /// <summary>
        /// Матрица b
        /// </summary>
        private double[] b;
        /// <summary>
        /// Матрица x
        /// </summary>
        private double[] x;
        /// <summary>
        /// Параметр отвечающий за работу приложения, пока true расчет не закончен
        /// </summary>
        private bool isWait = false;
        /// <summary>
        /// Прямой метод гауса
        /// </summary>
        /// <param name="a">матрица а</param>
        /// <param name="b">матрица b</param>
        /// <param name="n">размер матрицы n</param>
        /// <returns></returns>
        public double[] Gauss(double[][] a, double[] b, int n)
        {
            sw.Start();
            Console.WriteLine(">>>Start calculation<<<");
            double buf;
            double[] x = new double[n];
            int i, j, k;
            for (k = 0; k < n - 1; k++)
            {
                for (i = k + 1; i < n; i++)
                {
                    buf = a[i][k] / a[k][k];
                    for (j = k; j < n; j++)
                        a[i][j] = a[i][j] - (a[k][j] * buf);
                    b[i] = b[i] - (b[k] * buf);
                }
            }
            for (i = n - 1; i >= 0; i--)
            {
                x[i] = b[i] / a[i][i];
                for (j = 0; j < i; j++)
                    b[j] = b[j] - x[i] * a[j][i];
            }
            sw.Stop();
            string time = (Math.Round(sw.ElapsedMilliseconds / 1000.0, 8)).ToString();
            Console.WriteLine(">>>Finished calculation<<<");
            Console.WriteLine("Calculation time: " + time + " seconds");
            return x;
        }
        /// <summary>
        /// Распределенный метод гауса
        /// </summary>
        /// <param name="aIn">матрица а</param>
        /// <param name="bIn">матрица b</param>
        /// <param name="nIn">размер матрицы n</param>
        /// <param name="address">ip адрес на котром ждать клиентов</param>
        /// <param name="countSolver">количество клиентов</param>
        /// <returns></returns>
        public double[] GaussParalel(double[][] aIn, double[] bIn, int nIn, string address, int countSolver)
        {
            int i, j, k;
            a = new double[n][];
            for (i = 0; i < n; i++)
            {
                a[i] = new double[n + 1];
            }
            a = aIn;
            n = nIn;
            b = bIn;
            x = new double[n];
            int[] ports = new int[countSolver];
            this.countClient = countSolver;
            Console.WriteLine(">>>Waiting clients<<<");
            // подключение решателей
            for (i = 0; i < countSolver; i++)
            {
                ports[i] = 8080 + i;
                string args = "" + i + " " + address + " " + ports[i];
                getClient(ports[i], address, i);
            }
            sw.Start();
            Console.WriteLine(">>>Start calculation<<<");
            int l = 0;
            // Распределение и отправка пакетов клиентам
            for (k = 0; k < n; k++)
            {
                client = clients.ElementAt(l);
                stream = client.GetStream();
                matrix = new Matrix("", k, -1, -1, a[k], null);
                writeData(matrix, client);
                l++;
                if (l >= countClient)
                    l = 0;
            }
            // Завершение отправки, клиенты должны начать расчет
            sendMessageAllClient("", -1, null);

            // Отправка информации по элементам которые считать
            for (k = 0; k < n - 1; k++)
            {
                key = k;
                Console.WriteLine("Server send part " + key);
                sendMessageAllClient("Calculate package " + k, k, null);
                isWait = true;
                // ждем пока все посчитают пакет
                while (isWait) {
                    Thread.Sleep(10);
                }
            }
            // Завершение работы клиентов, они могут отключаться
            sendMessageAllClient("Stop", -1, null);
            while (!isEnd) { }
            // Обратный ход, получение всех х
            for (i = n - 1; i >= 0; i--)
            {
                x[i] = a[i][n] / a[i][i];
                for (j = 0; j < i; j++)
                    a[j][n] = a[j][n] - x[i] * a[j][i];
            }
            sendMessageAllClient("", -1, null);
            sw.Stop();
            string time = "time: " + (sw.ElapsedMilliseconds / 1000.0).ToString();
            Console.WriteLine(">>>Finished calculation<<<");
            Console.WriteLine("Calculation " + time + " seconds");
            return x;
        }
        /// <summary>
        /// метод отправки сообщений всем клиентам
        /// </summary>
        /// <param name="msg">сообщение</param>
        /// <param name="num">номер клиента</param>
        /// <param name="arr">массив данных</param>
        public void sendMessageAllClient(string msg, int num, double[] arr)
        {
            foreach (TcpClient clt in clients)
            {
                Matrix mt;
                // получение стрима и запись в него сериализлованных данных
                mt = new Matrix(msg, num, -1, -1, arr, null);
                writeData(mt, clt);
            }
        }
        /// <summary>
        /// Слушатель
        /// </summary>
        TcpListener listener;
        /// <summary>
        /// Колиичество клиентов
        /// </summary>
        int countClient = 0;
        /// <summary>
        /// Список всех клиентов
        /// </summary>
        List<TcpClient> clients = new List<TcpClient>();
        /// <summary>
        /// Подключение клиента
        /// </summary>
        /// <param name="port">порт</param>
        /// <param name="address">ip</param>
        /// <param name="deep">номер</param>
        public void getClient(int port, string address, int deep)
        {
            // вешаем слушателя для ожидания подключения клиента
            listener = new TcpListener(IPAddress.Parse(address), port);
            listener.Start();
            TcpClient client = listener.AcceptTcpClient();
            listener.Stop();
            matrix = null;
            while(matrix == null)
            {
                matrix = readData(client);
            }
            // добавляем клиента в поток ожидания сообщений от него
            ThreadPool.QueueUserWorkItem(new WaitCallback(getMessage), client);
            Console.WriteLine("Client " + matrix.Message + " connected on " + matrix.Index);
            if (a.Length % countClient >= deep + 1)
            {
                matrix = new Matrix("", 0, a.Length + 1, (a.Length / countClient) + 1, null, null);
            }
            else
            {
                matrix = new Matrix("", 0, a.Length + 1, a.Length / countClient, null, null);
            }
            writeData(matrix, client);
            // добовляем клиента в список
            clients.Add(client);
        }
        /// <summary>
        /// количество полученных расчетов (пакетов)
        /// </summary>
        private int pocitiveRes = 0;
        private int pocitiveCalc = 0;
        private bool isEnd = false;
        /// <summary>
        /// Метод обработки полученного сообщения от клиента
        /// </summary>
        /// <param name="client">клиент</param>
        public void getMessage(Object client)
        {
            Matrix matrix;
            // Пока true клиент работает, как false все посчитал
            isEnd = false;
            while (!isEnd)
            {
                try
                {
                    // проверка сколько клиентов все посчитали и вернули все
                    if (pocitiveRes == countClient)
                    {
                        isEnd = true;
                        continue;
                    }
                    // сколько клиентов посчитали свои данные с пакетом от другого клиента
                    if (pocitiveCalc == countClient)
                    {
                        isWait = false;
                        pocitiveCalc = 0;
                        continue;
                    }
                    // получение данны
                    matrix = readData((TcpClient)client);
                    if (matrix == null || matrix.Message == null)
                    {
                        continue;
                    }
                    // если клиент отправил промежуточное рассчитанное сообщение переотправляем его всем
                    if (matrix.Index == key)
                    {
                        sendMessageAllClient("", key, matrix.ArrayI);
                    }
                    // если клиент отключился
                    if (matrix.Index == -3)
                    {
                        Console.WriteLine("Server read all calculation parts from {0}", matrix.Message);
                        Console.WriteLine("{0} disconnect", matrix.Message);
                        for (int i = 0; i < matrix.ArrayI.Length; i++) 
                        {
                            int num = Convert.ToInt32(matrix.ArrayI[i]);
                            a[num] = matrix.Array[i];
                        }
                        pocitiveRes++;
                    }
                    // если клиент посчитал все пакеты с полученным
                    if (matrix.Message.IndexOf("p") >= 0 && matrix.Index == -2)
                    {
                        Console.WriteLine(matrix.Message + " calculation data");
                        pocitiveCalc++;
                    }
                }
                catch (Exception e)
                {
                    break;
                }
            }
        }
        /// <summary>
        /// метод отправки данных в стрим
        /// </summary>
        /// <param name="matrix">объект матрицы</param>
        /// <param name="client">клиент которому отсылать</param>
        public void writeData(Matrix matrix, TcpClient client)
        {   
            stream = client.GetStream();
            writer = new StreamWriter(stream, Encoding.UTF8);
            string jdata = "";
            jdata = JsonConvert.SerializeObject(matrix);
            writer.WriteLine(jdata);
            writer.Flush();
        }
        /// <summary>
        /// Метод получения данных из стрима
        /// </summary>
        /// <param name="client">Клиент от которого получать данные</param>
        /// <returns>считанная матрица данных</returns>
        public Matrix readData(TcpClient client)
        {
            stream = client.GetStream();
            reader = new StreamReader(stream, Encoding.UTF8);
            string json;
            json = reader.ReadLine();
            while (json != "" && json != null && json[0] != '{')
            {
                json = json.Remove(0, 1);
            }
            
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
