

using System;

namespace GenshinbotCsharp
{
    class Program
    {
    


        [STAThread]
        static void Main(string[] args)
        {
            Screenshot.Init();
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
            controllers.LocationManager.Test();
        }

    }
}
