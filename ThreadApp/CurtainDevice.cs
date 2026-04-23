using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ThreadApp;

namespace ThreadApp
{
    class CurtainDevice
    {
        static string position = "closed";

        public static void Run()
        {
            UdpClient client = new UdpClient();
            IPAddress myIP = NetworkHelper.GetLocalIPAddress();
            string serverIP = myIP.ToString();

            NetworkHelper.Send(client, "REGISTER|CURTAIN", serverIP, 1234);

            while (true)
            {
                IPEndPoint remote = null;
                var data = client.Receive(ref remote);
                string msg = Encoding.UTF8.GetString(data);

                Console.WriteLine($"[CURTAIN] Получено: {msg}");

                if (msg.StartsWith("COMMAND"))
                {
                    string cmd = msg.Split('|')[1];

                    if (cmd == "open" || cmd == "closed" || cmd == "half_open")
                    {
                        position = cmd;
                    }
                    else
                    {
                        Console.WriteLine("[CURTAIN] Ошибка: неизвестная команда");
                    }
                }

                if (msg == "GET_STATUS" || msg.StartsWith("COMMAND"))
                {
                    string status = $"STATUS|CURTAIN|position={position}";

                    byte[] response = Encoding.UTF8.GetBytes(status);
                    client.Send(response, response.Length, remote);

                    Console.WriteLine($"[CURTAIN] Отправлено: {status}");
                }
            }
        }
    }
}