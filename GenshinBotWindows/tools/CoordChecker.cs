using OpenCvSharp;
using System;
namespace genshinbot.tools
{
    class CoordChecker
    {
        const string winName = "screenshot";

        static Mat screenshot;
        public static void run(string[] args)
        {


          /* var g = GenshinWindow.FindExisting();// ("*Untitled - Notepad", null);
            g.InitHooking();
            Console.WriteLine("genshin window initted");

            Cv2.NamedWindow(winName);
            Cv2.SetMouseCallback(winName, onMouse);
            Console.WriteLine("press g to take screenshot");
            while (true)
            {
                var d = g.GetRectDirect();
                
                Console.WriteLine("window size detected: " + d.Size);

                 var mt = Screenshot.GetBuffer(d.Width, d.Height);
                tmp = new Mat(mt.Mat.Size(), mt.Mat.Type());
                while (g.GetRectDirect().Size == d.Size)
                {
                    var e = g.WaitKeyboardEvent();
                    if (e.KeyCode == VirtualKeyCode.VK_G && e.KbType == KeyboardEvent.KbEvtType.DOWN)
                    {
                        Console.WriteLine("taking screenshot");
                        g.TakeScreenshot(0, 0, mt);
                        screenshot = mt.Mat;
                        show();
                    }

                }
            }*/
        }

        private static int x, y;
        private static Mat tmp;
        private static void show()
        {
            if (screenshot != null)
            {
                Cv2.CopyTo(screenshot, tmp);
                Cv2.PutText(tmp, "mouse: " + x + ", " + y,
                    new Point(0, 100), HersheyFonts.HersheyPlain, 1, Scalar.White,2);
                Cv2.PutText(tmp, "window size:" + tmp.Width + ", " + tmp.Height,
                    new Point(0, 120), HersheyFonts.HersheyPlain, 1, Scalar.White,2);
                Cv2.ImShow(winName, tmp);
                Cv2.WaitKey(1);
            }
        }

        private static void onMouse(MouseEventTypes @event, int x1, int y1, MouseEventFlags flags, IntPtr userData)
        {
            if (tmp == null) return;
            var r=Cv2.GetWindowImageRect(winName);
            x = x1; y = y1;
            show();
        }
    }
}
