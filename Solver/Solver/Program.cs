using System;

namespace Solver
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Default номер
                int clientIndex = 0;
                // Default ip
                string address = "127.0.0.1";
                // Default port
                int port = 8080;
                // если пердали параметры для подключения, сохрянаем их
                if (args.Length > 0)
                {
                    clientIndex = int.Parse(args[0]);
                    address = args[1];
                    port = int.Parse(args[2]);
                }
                // если данных нет, вводим с клавиатуры
                else
                {
                    Console.Write("Client id:");
                    clientIndex = int.Parse(Console.ReadLine());
                    Console.Write("Client ip:");
                    address = Console.ReadLine();
                    Console.Write("Client port:");
                    port = int.Parse(Console.ReadLine());
                }    
                Calculation calculation = new Calculation();
                // подключаем клиента по ip, port и номеру
                calculation.connect(clientIndex, address, port);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
