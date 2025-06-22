using BhapticsTactsuit;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Bhaptics.Tact;

namespace bHapticsServer
{
    internal class Program
    {
        public static int udpPort = 5015;
        public static TactsuitVR tactsuitVR;

        static void Main(string[] args)
        {
            var arguments = ParseArguments(args);
            arguments.TryGetValue("appId", out string appId);
            appId = appId ?? "bHapticsMod";
            Console.WriteLine("App ID: " + appId);
            arguments.TryGetValue("appName", out string appName);
            appName = appName ?? "bHapticsMod";
            Console.WriteLine("App Name: " + appName);

            Console.Title = appName;

            tactsuitVR = new TactsuitVR(appId, appName);
            CreateUdpServer();
        }

        static void CreateUdpServer()
        {
            using (UdpClient udpServer = new UdpClient(new IPEndPoint(IPAddress.Loopback, udpPort)))
            {
                Console.WriteLine($"UDP Server started listening on 127.0.0.1:{udpPort}");

                while (true)
                {
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] receivedBytes = udpServer.Receive(ref remoteEP);
                    string receivedText = Encoding.UTF8.GetString(receivedBytes);
                    HandleMessage(receivedText);
                }
            }
        }

        static void HandleMessage(string message)
        {
            Console.WriteLine($"Received message: {message}");

            if (message.Contains("protube"))
            {
                HandleProtubeMessage(message);
            }
            else
            {
                HandleBhapticsMessage(message);
            }
        }

        static void HandleBhapticsMessage(string message)
        {
            if(message.Contains(","))
            {
                string[] paramshaptic = message.Split(',');                
                float intensity = float.TryParse(paramshaptic[1], out float res) ? res : 1f;
                float duration = float.TryParse(paramshaptic[2], out float res2) ? res2 : 1f;
                float offsetX = float.TryParse(paramshaptic[3], out float res3) ? res3 : 0;
                float offsetY = float.TryParse(paramshaptic[4], out float res4) ? res4 : 0;
                RotationOption rotation = new RotationOption(offsetX, offsetY);

                tactsuitVR.PlaybackHaptics(paramshaptic[0], true, intensity, duration, rotation);
                Console.WriteLine("Playing effect with params: " + message);
            }
            else
            {
                tactsuitVR.PlaybackHaptics(message);
                Console.WriteLine("Playing effect: " +  message);
            }
        }

        // protube=method.kickvalue.rumblevalue.rumbleduration.channel
        static void HandleProtubeMessage(string message)
        {
            if(message == "protubeinit")
            {
                ForceTubeVRInterface.InitAsync(true);
                Console.WriteLine("Initializing Protube devices");
            }
            else
            {
                string[] paramshaptic = message.Split('=')[1].Split('.');
                int kickPower = int.TryParse(paramshaptic[1], out int res) ? res : 0;
                float rumblePower = float.TryParse(paramshaptic[2], out float res2) ? res2 : 0f;
                float rumbleDuration = float.TryParse(paramshaptic[3], out float res3) ? res3 : 0f;
                int channel = int.TryParse(paramshaptic[4], out int res4) ? res4 : 0;

                switch (paramshaptic[0])
                {
                    case "kick":
                        ForceTubeVRInterface.Kick((byte)kickPower, (ForceTubeVRChannel)channel);
                        Console.WriteLine($"Protube Kick with params: {message}");
                        break;
                    case "shoot":
                        ForceTubeVRInterface.Shoot((byte)kickPower, (byte)rumblePower, (byte)rumbleDuration, (ForceTubeVRChannel)channel);
                        Console.WriteLine($"Protube Rumble with params: {message}");
                        break;
                    case "rumble":
                        ForceTubeVRInterface.Rumble((byte)rumblePower, (byte)rumbleDuration, (ForceTubeVRChannel)channel);
                        Console.WriteLine($"Protube Shoot with params: {message}");
                        break;
                    default:
                        break;
                }
            }
        }

        static Dictionary<string, string> ParseArguments(string[] args)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (arg.StartsWith("-"))
                {
                    string key = arg.TrimStart('-');

                    if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                    {
                        result[key] = args[i + 1];
                        i++;
                    }
                    else
                    {
                        result[key] = "true";
                    }
                }
            }

            return result;
        }
    }
}
