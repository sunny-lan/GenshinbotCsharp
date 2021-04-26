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
        static ConcurrentQueue<Action> run=new ConcurrentQueue<Action>();
        private static Task tsk;

        public static void Run()
        {
            while (running)
            {
                while (!run.IsEmpty)
                {
                    Debug.Assert(run.TryDequeue(out var action));
                    action();
                }
                Cv2.WaitKey(1);
            }
        }

        public static void ImShow(string name, Mat m)
        {
            Invoke(() => Cv2.ImShow(name, m));
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
