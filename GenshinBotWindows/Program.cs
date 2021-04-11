

using System;
using System.Threading;

namespace genshinbot
{
    class Program
    {
    


        [STAThread]
        static void Main(string[] args)
        {
            var wnd = new WindowAutomator("*Untitled - Notepad",null);
            while (true)
            {
                wnd.KeyPress((int)automation.input.Keys.M);
                Thread.Sleep(1000);
            }
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
            //controllers.LocationManager.Testwalkto();
            //screens.PlayingScreen.test();
              tools.Goto.Run();
            //screens.MapScreen.Test();
            // algorithm.CharacterPaletteRead.Test();
            // yui.WindowsForms.MainForm.Test();
            //experiments.XboxInput.Run();
            //GenshinBot.generalTest();
            //screens.PlayingScreen.TestRead();
        }

    }
}
