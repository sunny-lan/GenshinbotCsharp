

using genshinbot.automation;
using genshinbot.automation.windows;
using genshinbot.data;
using genshinbot.diag;
using genshinbot.util;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vanara.PInvoke;

namespace genshinbot
{
    class Program
    {
    


        [STAThread]
        static async Task Main(string[] args)
        {

            DPIAware.Set(SHCore.PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE);
            //TaskExceptionCatcher.Do ();
            Kernel32.AllocConsole();
            /*IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;

            User32.SetWindowLong(handle, User32.WindowLongFlags.GWL_EXSTYLE,
              (IntPtr)((uint)User32.GetWindowLong(handle,User32.WindowLongFlags.GWL_EXSTYLE)
              |(uint)User32.WindowStylesEx.WS_EX_NOACTIVATE |(uint)User32.WindowStylesEx.WS_EX_TOPMOST));
            Console.ReadLine();
            */

            var rig = new TestingRig();
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
            // automation.screenshot.directx.Test.Run();
            // automation.screenshot.gdi.run();
            // automation.screenshot.gdi.GDIStream.Test2();
            //automation.windows.WindowAutomator2.Test();
            //await automation.windows.WindowAutomator2.Test2();
            // await tools.AutofillTool.Test(rig);
            //   automation.windows.WindowAutomator2.Test3();
            //screens.PlayingScreen.test();
            //screens.MapScreen.Test2(rig);
            //await controllers.LocationManager.testAsync(rig);
            // await controllers.LocationManager.testAsync3();
            //await screens.MapScreen.Test3Async();
             automation.windows.WindowAutomator2.Test4();
            //Console.WriteLine(Data.General.Root.Find("derpity.derp").Points.Keys);
            //await tools.ScreencoordRecorder.runAsync(new WindowAutomator2("*Untitled - Notepad", null));
            // await tools.ScreencoordRecorder.runAsync(rig.Make().W);
            //// .Show();
            //Application.Run(new yui.windows.Overlay(rig));

            //await tools.DailyDoer.runAsync(rig.Make());
            // await tools.DailyDoer.DispatchCollect(rig.Make());
            //await experiments.RxTest.runAsync();
            //await tools.AutofillTool.ConfigureDailyDoer(rig.Make());
         //   await tools.AutofillTool.Test2();
            //await tools.AutofillTool.ConfigurePlayingScreen(rig.Make());
            //data.jsonconverters.MatConverter.Test();
            Console.WriteLine("Program ended. Press enter to exit");
            Console.ReadLine();
            CvThread.Stop();
        }

    }
}
