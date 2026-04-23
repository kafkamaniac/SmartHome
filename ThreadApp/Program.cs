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


    static bool running = true;
    static IPAddress myIP = NetworkHelper.GetLocalIPAddress();
    static int port = 1234;
    static void Main()
    {
        Thread serverThread = new Thread(Server);
        Thread clientThread = new Thread(ThreadApp.Client.Run);

        serverThread.Start();
        clientThread.Start();
    }
    static void Server()
    {
        UdpClient server = new UdpClient(1234);
        IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);

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
        if (parts.Length < 3) return;

        string type = parts[0];
        string device = parts[1];
        string data = parts[2];

        if (type != "COMMAND") return;

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
                targetTemperature = int.Parse(data.Split('=')[1]);
            }

            Console.WriteLine($"[SERVER] Target temp = {targetTemperature}");
        }
    }

    static void AlarmSystem()
    {
        UdpClient client = new UdpClient();
        Random rand = new Random();

        bool alarmActive = false;

        while (running)
        {
            if (alarmEnabled)
            {
                bool motionDetected = rand.Next(0, 10) > 7;

                if (motionDetected && !alarmActive)
                {
                    alarmActive = true;
                    NetworkHelper.Send(client, "ALARM: движение обнаружено", myIP.ToString(), port);
                }
            }

            Thread.Sleep(1000);
        }
    }

    static void ThermostatSystem()
    {
        UdpClient client = new UdpClient();
        Random rand = new Random();

        int temperature = 20;
        bool heating = false;

        while (running)
        {
            if (temperature < targetTemperature && !heating)
            {
                heating = true;
                NetworkHelper.Send(client, "ТЕРМОСТАТ: обогрев ВКЛ", myIP.ToString(), port);
            }
            else if (temperature >= targetTemperature && heating)
            {
                heating = false;
                NetworkHelper.Send(client, "ТЕРМОСТАТ: обогрев ВЫКЛ", myIP.ToString(), port);
            }

            Thread.Sleep(1000);
        }
    }

}