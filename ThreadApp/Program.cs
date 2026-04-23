using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ThreadApp;

class Program
{
    static bool alarmEnabled = false;
    static int targetTemperature = 18;
    static int currentTemperature = 20;

    static UdpClient server;
    static IPEndPoint remote;

    static bool running = true;
    static IPAddress myIP = NetworkHelper.GetLocalIPAddress();
    static int port = 1234;

    static void Main()
    {
        Thread serverThread = new Thread(Server);
        Thread clientThread = new Thread(ThreadApp.Client.Run);

        serverThread.Start();
        clientThread.Start();

        serverThread.Join();
        clientThread.Join();
    }

    static void Server()
    {
        server = new UdpClient(1234);
        remote = new IPEndPoint(IPAddress.Any, 0);

        Console.WriteLine($"Сервер запущен на {myIP}:{1234}");

        while (running)
        {
            if (server.Available > 0)
            {
                byte[] data = server.Receive(ref remote);
                string message = Encoding.UTF8.GetString(data);

                Console.WriteLine($"[СЕРВЕР] {remote.Address}: {message}");

                HandleMessage(message);
            }
            else
            {
                Thread.Sleep(50);
            }
        }
    }

    static void HandleMessage(string message)
    {
        string[] parts = message.Split('|');

        if (parts[0] == "GET_STATUS")
        {
            string status = $"ALARM|{alarmEnabled}|THERMOSTAT|target={targetTemperature},current={currentTemperature}";

            Console.WriteLine($"[SERVER] Отправка статуса: {status}");

            byte[] someData = Encoding.UTF8.GetBytes(status);
            server.Send(someData, someData.Length, remote);
            return;
        }

        if (parts[0] != "COMMAND" || parts.Length < 3) return;

        string device = parts[1];
        string data = parts[2];

        if (device == "ALARM")
        {
            if (data == "on") alarmEnabled = true;
            if (data == "off") alarmEnabled = false;

            Console.WriteLine($"[SERVER] Alarm state = {alarmEnabled}");
        }

        if (device == "THERMOSTAT")
        {
            if (data.StartsWith("set="))
            {
                currentTemperature = int.Parse(data.Split('=')[1]);
            }

            Console.WriteLine($"[SERVER] Target temp = {currentTemperature}");
        }
    }
}