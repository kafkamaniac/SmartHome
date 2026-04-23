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
    using System.Threading;

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
                "Сигнализация ВКЛ",
                "Сигнализация ВЫКЛ",
                "Установить температуру",

                "Свет ВКЛ",
                "Свет ВЫКЛ",
                "Установить яркость",

                "Шторы открыть",
                "Шторы закрыть",
                "Шторы наполовину",

                "Увлажнитель ВКЛ",
                "Увлажнитель ВЫКЛ",
                "Установить влажность",

                "Показать состояние устройств",
                "Выход"
            };

            while (true)
            {
                int selected = Menu(menuItems);

                string message = "";
                bool needResponse = false;

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
                        message = $"COMMAND|THERMOSTAT|set={temp}";
                        break;

                    case 3:
                        message = "COMMAND|LIGHT|on";
                        break;

                    case 4:
                        message = "COMMAND|LIGHT|off";
                        break;

                    case 5:
                        Console.Write("\nВведите яркость (0–100): ");
                        string bright = Console.ReadLine();
                        message = $"COMMAND|LIGHT|brightness={bright}";
                        break;

                    case 6:
                        message = "COMMAND|CURTAIN|open";
                        break;

                    case 7:
                        message = "COMMAND|CURTAIN|closed";
                        break;

                    case 8:
                        message = "COMMAND|CURTAIN|half_open";
                        break;

                    case 9:
                        message = "COMMAND|HUMIDIFIER|on";
                        break;

                    case 10:
                        message = "COMMAND|HUMIDIFIER|off";
                        break;

                    case 11:
                        Console.Write("\nВведите влажность (30–80): ");
                        string hum = Console.ReadLine();
                        message = $"COMMAND|HUMIDIFIER|humidity={hum}";
                        break;

                    case 12:
                        message = "GET_STATUS";
                        needResponse = true;
                        break;

                    case 13:
                        return;
                }

                NetworkHelper.Send(client, message, serverIP, port);

                if (needResponse)
                {
                    try
                    {
                        client.Client.ReceiveTimeout = 3000;
                        IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                        byte[] buffer = client.Receive(ref remote);
                        string response = Encoding.UTF8.GetString(buffer);

                        Console.WriteLine("\n=== Состояние устройств ===");
                        string[] parts = response.Split('|');
                        for (int i = 0; i < parts.Length; i++)
                        {
                            if (parts[i] == "ALARM")
                            {
                                bool isEnabled = bool.Parse(parts[i + 1]);
                                Console.WriteLine($"Сигнализация: {(isEnabled ? "ВКЛ" : "ВЫКЛ")}");
                            }
                            if (parts[i] == "THERMOSTAT")
                            {
                                string[] thermostatData = parts[i + 1].Split(',');
                                string target = thermostatData[0].Split('=')[1];
                                string current = thermostatData[1].Split('=')[1];
                                Console.WriteLine($"Целевая температура: {target}");
                                Console.WriteLine($"Текущая температура: {current}");
                            }

                            if (parts[i] == "LIGHT")
                            {
                                string[] data = parts[i + 1].Split(',');
                                bool on = bool.Parse(data[0].Split('=')[1]);
                                string brightness = data[1].Split('=')[1];

                                Console.WriteLine($"Свет: {(on ? "ВКЛ" : "ВЫКЛ")}, яркость {brightness}");
                            }

                            if (parts[i] == "CURTAIN")
                            {
                                string pos = parts[i + 1].Split('=')[1];
                                Console.WriteLine($"Шторы: {pos}");
                            }

                            if (parts[i] == "HUMIDIFIER")
                            {
                                string[] data = parts[i + 1].Split(',');
                                bool on = bool.Parse(data[0].Split('=')[1]);
                                string hum = data[1].Split('=')[1];

                                Console.WriteLine($"Увлажнитель: {(on ? "ВКЛ" : "ВЫКЛ")}, влажность {hum}");
                            }
                        }
                        Console.WriteLine("===========================");
                        Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                        Console.ReadKey();
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine("Ошибка: сервер не ответил в течение 3 секунд");
                        Console.WriteLine("Нажмите любую клавишу для продолжения...");
                        Console.ReadKey();
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        static int Menu(string[] items)
        {
            int index = 0;
            ConsoleKey key;

            do
            {
                Console.Clear();
                Console.WriteLine("Добро пожаловать в систему умный дом!\n");

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