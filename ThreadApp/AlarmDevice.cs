using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ThreadApp
{
    internal class AlarmDevice
    {
        static bool alarmEnabled = false;

        static string alarmMode = "disarmed";

        public static void Run()
        {
            UdpClient client = new UdpClient();
            IPAddress myIP = NetworkHelper.GetLocalIPAddress();
            string serverIP = myIP.ToString();

            NetworkHelper.Send(client, "REGISTER|ALARM", serverIP, 1234);

            while (true)
            {
                IPEndPoint remote = null;
                var data = client.Receive(ref remote);
                string msg = Encoding.UTF8.GetString(data);

                Console.WriteLine($"[ALARM] Получено: {msg}");

                string[] parts = msg.Split('|');

                if (parts[0] == "COMMAND")
                {
                    string cmd = parts[1];

                    if (cmd == "on")
                    {
                        alarmEnabled = true;
                    }
                    else if (cmd == "off")
                    {
                        alarmEnabled = false;
                    }


                    else if (cmd.StartsWith("mode="))
                    {
                        string newMode = cmd.Split('=')[1];

                        if (newMode == "armed" || newMode == "disarmed" || newMode == "triggered")
                        {
                            alarmMode = newMode;
                        }
                        else
                        {
                            Console.WriteLine("[ALARM] Ошибка: неизвестный режим");
                        }
                    }
                }

                if (msg == "GET_STATUS" || parts[0] == "COMMAND")
                {
                    string status = $"STATUS|ALARM|enabled={alarmEnabled},mode={alarmMode}";

                    byte[] response = Encoding.UTF8.GetBytes(status);
                    client.Send(response, response.Length, remote);

                    Console.WriteLine($"[ALARM] Отправлено: {status}");
                }

            }
        }
    }
}
