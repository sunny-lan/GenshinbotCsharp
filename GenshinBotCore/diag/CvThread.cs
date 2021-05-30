using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.diag
{
    public class CvThread
    {
        static bool running;
        static ConcurrentQueue<Action> run = new ConcurrentQueue<Action>();
        private static Task tsk;
        static Dictionary<string, Mat> updates = new Dictionary<string, Mat>();

        public static double MaxFps = 24;

        public static void Run()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (running)
            {
                lock (updates)
                {
                    foreach (var entry in updates)
                    {
                        Cv2.ImShow(entry.Key, entry.Value);
                    }
                    updates.Clear();
                }

                while (!run.IsEmpty)
                {
                    Debug.Assert(run.TryDequeue(out var action));
                    action();
                }

                var msPerFrame = 1000 / MaxFps;
                Cv2.WaitKey((int)Math.Max(1, msPerFrame - sw.ElapsedMilliseconds));
                sw.Restart();
            }
        }

        public static void ImShow(string name, Mat m)
        {
            lock (updates)
                updates[name] = m;
        }

        public static void Invoke(Action a)
        {
            run.Enqueue(a);
        }

        public static void Stop()
        {
            if (!running) return;
            running = false;
            tsk.Wait();
        }

        static CvThread()
        {
            running = true;
            tsk = Task.Run(Run);

        }
    }
}
