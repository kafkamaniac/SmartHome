using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreadApp
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    class Client
    {
        public static void Run()
        {
            Console.OutputEncoding = Encoding.UTF8;

            IPAddress myIP = NetworkHelper.GetLocalIPAddress();
            string serverIP = myIP.ToString();

            int port = 1234;
            UdpClient client = new UdpClient();

            string[] menuItems =
            {
            "Включить сигнализацию",
            "Выключить сигнализацию",
            "Установить температуру",
            "Выход"
        };

            while (true)
            {
                int selected = Menu(menuItems);

                string message = "";

                switch (selected)
                {
                    case 0:
                        message = "COMMAND|ALARM|on";
                        break;

                    case 1:
                        message = "COMMAND|ALARM|off";
                        break;

                    case 2:
                        Console.Write("\nВведите температуру: ");
                        string temp = Console.ReadLine();
                        message = $"COMMAND|THERMOSTAT|set{temp}";
                        break;

                    case 3:
                        return;
                }

                NetworkHelper.Send(client, message, serverIP, port);
            }

            static int Menu(string[] items)
            {
                int index = 0;
                ConsoleKey key;

                do
                {
                    Console.Clear();
                    Console.WriteLine("_Добро пожаловать в систему умный дом!_\n");

                    for (int i = 0; i < items.Length; i++)
                    {
                        if (i == index)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"> {items[i]}");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.WriteLine($"  {items[i]}");
                        }
                    }

                    key = Console.ReadKey(true).Key;

                    if (key == ConsoleKey.UpArrow)
                    {
                        index--;
                        if (index < 0) index = items.Length - 1;
                    }
                    else if (key == ConsoleKey.DownArrow)
                    {
                        index++;
                        if (index >= items.Length) index = 0;
                    }

                } while (key != ConsoleKey.Enter);

                return index;
            }

        }
    }
}
