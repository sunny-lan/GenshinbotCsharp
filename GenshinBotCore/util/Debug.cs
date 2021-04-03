using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace genshinbot
{
    public class Debug
    {
        public static Mat img
        {
            get
            {
                throw new NotSupportedException();
                lock (locker) return img1;
            }
            set
            {
                throw new NotSupportedException();
                lock (locker) img1 = value;
            }
        }
        private static Mat img1 = new Mat();
        public static object locker = new object();
        public static int key = -1;
        private static object waitInit = new object();
        private static bool inited = false;
        static Debug()
        {
           /* Task.Run(() =>
            {
               // Cv2.NamedWindow("debug", WindowMode.KeepRatio);
                lock (waitInit)
                {
                    inited = true;
                    Monitor.PulseAll(waitInit);
                }
                while (true)
                {
                    key = Cv2.WaitKey();
                    lock (locker)
                    {
                        Monitor.PulseAll(locker);
                        key = -1;
                    }
                }
            });*/
        }

        public static void show()
        {
            throw new NotSupportedException();
            lock (waitInit)
            {
                if(!inited)
                Monitor.Wait(waitInit);
                Cv2.ImShow("debug", img);
            }
        }
        public static void show(string  name, Mat img)
        {
            throw new NotSupportedException();
            Cv2.ImShow(name, img);
            Cv2.WaitKey(1);
        }

        public static int WaitKey()
        {
            throw new NotSupportedException();
            lock (locker)
            {
                Monitor.Wait(locker);
                return key;
            }
        }

        [System.Diagnostics.DebuggerHidden]
        public static void Assert(bool b, string s = "assert failed")
        {
            if (!b) throw new Exception(s);
        }
    }
}
