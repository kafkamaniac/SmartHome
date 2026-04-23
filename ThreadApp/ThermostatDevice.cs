using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ThreadApp;

namespace ThreadApp
{
    class ThermostatDevice
    {
        static int target = 20;
        static int current = 18;
        static string mode = "auto";

        public static void Run()
        {
            UdpClient client = new UdpClient();
            IPAddress myIP = NetworkHelper.GetLocalIPAddress();
            string serverIP = myIP.ToString();

            NetworkHelper.Send(client, "REGISTER|THERMOSTAT", serverIP, 1234);

            while (true)
            {
                IPEndPoint remote = null;
                var data = client.Receive(ref remote);
                string msg = Encoding.UTF8.GetString(data);

                Console.WriteLine($"[THERMOSTAT] Получено: {msg}");

                if (msg.StartsWith("COMMAND"))
                {
                    string cmd = msg.Split('|')[1];

                    if (cmd.StartsWith("set="))
                    {
                        int value = int.Parse(cmd.Split('=')[1]);

                        if (value >= 16 && value <= 30)
                        {
                            current = value;
                        }
                        else
                        {
                            Console.WriteLine("[THERMOSTAT] Ошибка: температура вне диапазона (16–30)");
                        }
                    }

                    else if (cmd.StartsWith("mode="))
                    {
                        string newMode = cmd.Split('=')[1];

                        if (newMode == "auto" || newMode == "heat" || newMode == "cool")
                        {
                            mode = newMode;
                        }
                        else
                        {
                            Console.WriteLine("[THERMOSTAT] Ошибка: неизвестный режим");
                        }
                    }
                }

                if (msg == "GET_STATUS" || msg.StartsWith("COMMAND"))
                {
                    string status = $"STATUS|THERMOSTAT|target={target},current={current},mode={mode}";

                    byte[] response = Encoding.UTF8.GetBytes(status);
                    client.Send(response, response.Length, remote);

                    Console.WriteLine($"[THERMOSTAT] Отправлено: {status}");
                }
                
            }
        }
    }
}
