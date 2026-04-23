using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ThreadApp;

namespace ThreadApp
{
    class HumidifierDevice
    {
        static bool isOn = false;
        static int humidity = 50;

        public static void Run()
        {
            UdpClient client = new UdpClient();
            IPAddress myIP = NetworkHelper.GetLocalIPAddress();
            string serverIP = myIP.ToString();

            NetworkHelper.Send(client, "REGISTER|HUMIDIFIER", serverIP, 1234);

            while (true)
            {
                IPEndPoint remote = null;
                var data = client.Receive(ref remote);
                string msg = Encoding.UTF8.GetString(data);

                Console.WriteLine($"[HUMIDIFIER] Получено: {msg}");

                if (msg.StartsWith("COMMAND"))
                {
                    string cmd = msg.Split('|')[1];

                    if (cmd == "on")
                    {
                        isOn = true;
                    }
                    else if (cmd == "off")
                    {
                        isOn = false;
                    }
                    else if (cmd.StartsWith("humidity="))
                    {
                        int value = int.Parse(cmd.Split('=')[1]);

                        if (value >= 30 && value <= 80)
                        {
                            humidity = value;
                        }
                        else
                        {
                            Console.WriteLine("[HUMIDIFIER] Ошибка: влажность вне диапазона (30–80)");
                        }
                    }
                }

                if (msg == "GET_STATUS" || msg.StartsWith("COMMAND"))
                {
                    string status = $"STATUS|HUMIDIFIER|on={isOn},humidity={humidity}";

                    byte[] response = Encoding.UTF8.GetBytes(status);
                    client.Send(response, response.Length, remote);

                    Console.WriteLine($"[HUMIDIFIER] Отправлено: {status}");
                }
            }
        }
    }
}