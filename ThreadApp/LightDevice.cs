using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ThreadApp;

namespace ThreadApp
{
    class LightDevice
    {
        static bool isOn = false;
        static int brightness = 50; 

        public static void Run()
        {
            UdpClient client = new UdpClient();
            IPAddress myIP = NetworkHelper.GetLocalIPAddress();
            string serverIP = myIP.ToString();

            NetworkHelper.Send(client, "REGISTER|LIGHT", serverIP, 1234);

            while (true)
            {
                IPEndPoint remote = null;
                var data = client.Receive(ref remote);
                string msg = Encoding.UTF8.GetString(data);

                Console.WriteLine($"[LIGHT] Получено: {msg}");

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
                    else if (cmd.StartsWith("brightness="))
                    {
                        int value = int.Parse(cmd.Split('=')[1]);

                        if (value >= 0 && value <= 100)
                        {
                            brightness = value;
                        }
                        else
                        {
                            Console.WriteLine("[LIGHT] Ошибка: яркость вне диапазона (0–100)");
                        }
                    }
                }

                if (msg == "GET_STATUS" || msg.StartsWith("COMMAND"))
                {
                    string status = $"STATUS|LIGHT|on={isOn},brightness={brightness}";

                    byte[] response = Encoding.UTF8.GetBytes(status);
                    client.Send(response, response.Length, remote);

                    Console.WriteLine($"[LIGHT] Отправлено: {status}");
                }
            }
        }
    }
}