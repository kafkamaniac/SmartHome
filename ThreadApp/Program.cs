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

    static Dictionary<string, IPEndPoint> devices = new();
    static void Main()
    {
        Thread serverThread = new Thread(Server);
        Thread clientThread = new Thread(ThreadApp.Client.Run);

        new Thread(ThermostatDevice.Run).Start();
        new Thread(AlarmDevice.Run).Start();
        new Thread(LightDevice.Run).Start();
        new Thread(CurtainDevice.Run).Start();
        new Thread(HumidifierDevice.Run).Start();

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

        if (parts[0] == "REGISTER")
        {
            string deviceName = parts[1];
            devices[deviceName] = remote;

            Console.WriteLine($"Устройство {deviceName} зарегистрировано");
        }


        if (parts[0] == "GET_STATUS")
        {
            foreach (var device in devices)
            {
                byte[] req = Encoding.UTF8.GetBytes("GET_STATUS");
                server.Send(req, req.Length, device.Value);
            }

            string status =
                $"ALARM|{alarmEnabled}|" +
                $"THERMOSTAT|target={targetTemperature},current={currentTemperature}";

            byte[] someData = Encoding.UTF8.GetBytes(status);
            server.Send(someData, someData.Length, remote);

            return;
        }

        if (parts[0] == "STATUS")
        {
            if (parts[1] == "ALARM")
            {
                string[] data = parts[2].Split(',');
                alarmEnabled = bool.Parse(data[0].Split('=')[1]);
            }

            if (parts[1] == "THERMOSTAT")
            {
                string[] data = parts[2].Split(',');
                targetTemperature = int.Parse(data[0].Split('=')[1]);
                currentTemperature = int.Parse(data[1].Split('=')[1]);
            }

            return;
        }

        if (parts[0] != "COMMAND" || parts.Length < 3) return;


        if (parts[0] == "COMMAND")
        {
            string device = parts[1];

            if (devices.ContainsKey(device))
            {
                string command = $"COMMAND|{parts[2]}";
                byte[] data = Encoding.UTF8.GetBytes(command);

                server.Send(data, data.Length, devices[device]);
            }
        }

    }
}