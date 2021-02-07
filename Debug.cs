using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenshinbotCsharp
{
    class Debug
    {
        public static Mat img = new Mat();
        public static object locker = new object();
        public static int key = -1;

        static Debug()
        {
            new Thread(() =>
            {
                Cv2.NamedWindow("debug", WindowMode.KeepRatio);
                while (true)
                {
                    key = Cv2.WaitKey();
                    lock (locker)
                    {
                        Monitor.PulseAll(locker);
                        key = -1;
                    }
                }
            }).Start();
        }

        public static void show()
        {
            Cv2.ImShow("debug", img);
        }

        public static int WaitKey()
        {
            lock (locker)
            {
                Monitor.Wait(locker);
                return key;
            }
        }

    }
}
