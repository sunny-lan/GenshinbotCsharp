

using genshinbot.diag;
using System;
using System.Threading;
using Vanara.PInvoke;

namespace genshinbot
{
    class Program
    {
    


        [STAThread]
        static void Main(string[] args)
        {
            TaskExceptionCatcher.Do ();
            Kernel32.AllocConsole();
            //   Screenshot.Init();
            //TestMapLive();
            // tools.CoordChecker.run(args);
            //TestMap();
            // Cv2.WaitKey();
            //tools.CoordRecorder.run(args);
            // TestLocationDetect();
            // MinimapMatcher.Test();
            //WinEventHook.Test();
            //WindowAutomator.Test();
            // var g = gui.GUI.Instance;
            // algorithm.MinimapMatch.ScaleMatcher.test();
            //controllers.LocationManager.Testwalkto();
            //screens.PlayingScreen.test();
            // tools.Goto.Run();
            //screens.MapScreen.Test();
            // algorithm.CharacterPaletteRead.Test();
            // yui.WindowsForms.MainForm.Test();
            //experiments.XboxInput.Run();
            //GenshinBot.generalTest();
            //screens.PlayingScreen.TestRead();
            // algorithm.ArrowDirectionDetect.Test();
            //stream.Poller.Test1();
            //  automation.screenshot.directx.Test.Run();
            // automation.screenshot.gdi.run();
            automation.screenshot.gdi.GDIStream.Test2();
            Console.WriteLine("Program ended. Press enter to exit");
            Console.ReadLine();
            CvThread.Stop();
        }

    }
}
