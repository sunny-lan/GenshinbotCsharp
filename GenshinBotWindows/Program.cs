﻿

using Autofac;
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

        class BaseBotIO : BotIO
        {
            public IWindowAutomator2 W { get; }

            public BaseBotIO(IWindowAutomator2 w)
            {
                W = w;
            }
        }

        static async Task Main(string[] args)
        {
            Process process = Process.GetCurrentProcess();
            process.PriorityClass = ProcessPriorityClass.AboveNormal;
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

            MockGenshinWindow mkMok()
            {
                var gw = new MockGenshinWindow(new OpenCvSharp.Size(1680, 1050));
                gw.MapScreen.Image = Data.Imread("test/map_luhua_1050.png");
                gw.PlayingScreen.Image = Data.Imread("test/playing_luhua_1050.png");
                gw.CurrentScreen = gw.PlayingScreen;
                return gw;
            }

            var builder = new ContainerBuilder();

            builder.Register<YUI>(_ => yui.windows.MainForm.make()).SingleInstance();
            builder.Register<IWindowAutomator2>(_ => new WindowAutomator2("Genshin Impact", "UnityWndClass")).SingleInstance();
            //builder.Register<IWindowAutomator2>(_ => mkMok()).SingleInstance();
            builder.RegisterType<BaseBotIO>().As<BotIO>().SingleInstance();
            builder.RegisterType<screens.ScreenManager>().SingleInstance();
            builder.RegisterType<controllers.LocationManager>().SingleInstance();
            builder.RegisterType<tools.AutofillTool>().SingleInstance();
            builder.RegisterType<tools.BlackbarFixer>().SingleInstance();
            builder.RegisterType<tools.DailyDoer>().SingleInstance();
            builder.RegisterType<tools.MapUI>().SingleInstance();
            builder.Register<tools.ToolSelectorUI>(sp => new tools.ToolSelectorUI(
                sp.Resolve<YUI>(), new object[]
                {
                        sp.Resolve<tools.DailyDoer>(),
                        sp.Resolve<tools.AutofillTool>(),
                        sp.Resolve<tools.BlackbarFixer>(),

                })).SingleInstance();
            builder.RegisterType<controllers.LocationManager.Test>().SingleInstance();
            
            var sp = builder.Build();

            //  await screens.MapScreen.Testshow(rig);

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
            //            await automation.screenshot.gdi.GDIStream.Test2();
            //automation.windows.WindowAutomator2.Test();
            //await automation.windows.WindowAutomator2.Test2();
            // await tools.AutofillTool.Test(rig);
            //   automation.windows.WindowAutomator2.Test3();
            //screens.PlayingScreen.test();
            //screens.MapScreen.Test2(rig);

            // await controllers.LocationManager.TestTrackAsync(rig);
            // await controllers.LocationManager.testAsync3();
            //await screens.MapScreen.Test3Async();
            // automation.windows.WindowAutomator2.Test4();
            //Console.WriteLine(Data.General.Root.Find("derpity.derp").Points.Keys);
            //await tools.ScreencoordRecorder.runAsync(new WindowAutomator2("*Untitled - Notepad", null));
            // await tools.ScreencoordRecorder.runAsync(rig.Make().W);
            //// .Show();
            //Application.Run(new yui.windows.aRRO()); 
            //await screens.PlayingScreen.Test3Async();
            //await screens.PlayingScreen.TestClimb();
            // screens.PlayingScreen.TestClimb2(rig);
            //await tools.DailyDoer.runAsync(rig.Make());
            // await tools.DailyDoer.DispatchCollect(rig.Make());
            //await experiments.RxTest.runAsync();
            //await tools.AutofillTool.ConfigureDispatch(rig.Make());
            //   await tools.AutofillTool.Test2();
            //await tools.AutofillTool.ConfigurePlayingScreen(rig.Make());
            //data.jsonconverters.MatConverter.Test();
            // await automation.windows.WindowAutomator2.TestKbdLock();
            //       await controllers.LocationManager.TestGoto(rig);
            // await tools.WalkRecorder.TestAsync(rig.Make());
            //  await tools.AutofillTool.ConfigureCharacterSel(rig.Make());
            var sm = sp.Resolve<screens.ScreenManager>();
            sm.ForceScreen(sm.PlayingScreen);
            //4  using var kk = sp.GetService<tools.WalkEditor>();
            var dd = sp.Resolve<tools.ToolSelectorUI>();
            sp.Resolve<tools.MapUI>();
            //  await sp.GetService<tools.AutofillTool>()!.ConfigureAll();
            //  await sp.Resolve<controllers.LocationManager.Test>().TestGoto();
            //  await sp.GetService<tools.BlackbarFixer>().FixBlackBar();
            //  await tools.DailyDoer.runAsync(rig.Make());
            //algorithm.ChatReadAlg.Test();
            Console.WriteLine("Program ended. Press enter to exit");
            Console.ReadLine();
            CvThread.Stop();
        }

    }
}
