using System;
using Bhaptics.SDK2;
using System.Threading;

namespace bHapticsServer
{
    public class TactsuitVR
    {
        private static ManualResetEvent HeartBeat_morse = new ManualResetEvent(false);

        public int heartbeatCount;
        public const int heartbeatMax = 4;
        
        public TactsuitVR()
        {
            Logger.Log("Initializing bHaptics suit!");
            var result = BhapticsSDK2.Initialize("698abca726ed978f93a4df8b", "nvra3XIknUllyu9uOkoI");

            if (result > 0)
            {
                Logger.Log("Failed to do bHaptics initialization...");
                return;
            }
            
            Logger.Log("Starting HeartBeat thread...");
            
            var HeartBeatThread = new Thread(HeartBeatFunc);
            HeartBeatThread.Start();
            
            StartHeartBeat();
        }
        
        public void HeartBeatFunc()
        {
            while (true)
            {
                HeartBeat_morse.WaitOne();
                PlaybackHaptics("HeartBeat");
                
                if (heartbeatCount > heartbeatMax)
                {
                    StopHeartBeat();
                }
                heartbeatCount++;
                Thread.Sleep(600);
            }
        }

        public static void PlaybackHaptics(string key, float intensity = 1.0f, float duration = 1.0f, float xzAngle = 0f, float yShift = 0f)
        {
            BhapticsSDK2.Play(key.ToLower(), intensity, duration, xzAngle, yShift);
            Console.WriteLine($"{key}, {intensity}, {duration}, {xzAngle}, {yShift}");
        }

        public void StartHeartBeat()
        {
            HeartBeat_morse.Set();
        }

        public void StopHeartBeat()
        {
            HeartBeat_morse.Reset();
            heartbeatCount = 0;
        }

        public bool IsPlaying(string effect)
        {
            return BhapticsSDK2.IsPlaying(effect.ToLower());
        }

        public void StopHapticFeedback(string effect)
        {
            BhapticsSDK2.Stop(effect.ToLower());
        }

        public void StopAllHapticFeedback()
        {
            StopThreads();
            BhapticsSDK2.StopAll();
        }

        public void StopThreads()
        {
            StopHeartBeat();
        }
    }
}
