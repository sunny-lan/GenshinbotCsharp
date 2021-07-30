

using Autofac;
using genshinbot.automation;
using genshinbot.automation.input;
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
    using WindowAutoFac = Func<string, string, IWindowAutomator2>;
    class Program
    {

        class BaseBotIO : BotIO
        {
            class MyKAdp:automation.proxy.KbdAdapter
            {
                Random rng = new();
                public MyKAdp(IKeySimulator2 wrap) : base(wrap)
                {
                }

                public override async Task KeyPress(automation.input.Keys k)
                {
                    await wrap.KeyDown(k);
                    await Task.Delay(rng.Next(5,15));
                    await wrap.KeyUp(k);

                }
            }
            public IWindowAutomator2 W { get; }

            public IMouseSimulator2 M { get; }
            public IKeySimulator2 K { get; }

            public BaseBotIO(IWindowAutomator2 w)
            {
                W = w;
                M = new WindMouseMover(w.Mouse);
                K = new MyKAdp(w.Keys);
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
            builder.Register<ArduinoAutomator>(_ =>
            {
                var s = System.IO.Ports.SerialPort.GetPortNames();
                Debug.Assert(s.Length == 1);
                    return new ArduinoAutomator(s[0],async () => Cursor.Position.Cv());
                
            }).SingleInstance().As<IMouseSimulator2>().As<IKeySimulator2>().AsSelf();
            builder.Register<WindowAutoFac> (sp => { 
                    var auto = sp.Resolve<IKeySimulator2>();
                    var auto1 = sp.Resolve<IMouseSimulator2>();
                IWindowAutomator2 factory(string a, string b)
                {
                    return new WindowAutomator2(
                    a,b, auto1, auto
                    );
                }
                return factory;
            }).SingleInstance(); 
            builder.RegisterType<WindowAutomator2.Test>().SingleInstance();
             builder.Register<IWindowAutomator2>(sp =>sp.Resolve<WindowAutoFac>()(
                "Genshin Impact",
                "UnityWndClass"
                )).SingleInstance();
            //builder.Register<IWindowAutomator2>(_ => mkMok()).SingleInstance();
            builder.RegisterType<BaseBotIO>().As<BotIO>().SingleInstance();
            builder.RegisterType<screens.ScreenManager>().SingleInstance();
            builder.Register(sp => sp.Resolve<screens.ScreenManager>().InventoryScreen);
            builder.Register(sp => sp.Resolve<screens.ScreenManager>().MapScreen);
            builder.Register(sp => sp.Resolve<screens.ScreenManager>().PlayingScreen);
            builder.RegisterType<controllers.LocationManager>().SingleInstance();
            builder.RegisterType<tools.AutofillTool>().As<tools.ITool>().SingleInstance();
            builder.RegisterType<tools.BlackbarFixer>().As<tools.ITool>().SingleInstance();
            builder.RegisterType<tools.DailyDoer>().As<tools.ITool>().SingleInstance();
            builder.RegisterType<tools.PotDoer>().As<tools.ITool>().SingleInstance();
            builder.RegisterType<tools.MapUI>().SingleInstance();
            builder.RegisterType<tools.ToolSelectorUI>().SingleInstance();
            builder.RegisterType<controllers.LocationManager.Test>().SingleInstance();
            builder.RegisterType<automation.ArduinoAutomator.Test>().SingleInstance();
            builder.RegisterType<WindMouseMover.Test>().SingleInstance();

            var sp = builder.Build();
            /*var sm1 = sp.Resolve<screens.ScreenManager>();

            while (true)
            {
                var thing = await sm1.ExpectOneOf(new screens.IScreen[] {
                        sm1.PlayingScreen,sm1.MapScreen });
                Console.WriteLine(thing.GetType().Name);
            }*/

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
            //await sp.Resolve<ArduinoAutomator.Test>().TestMove();
        //    await sp.Resolve<ArduinoAutomator.Test>().TestWindMove();
            //await sp.Resolve<WindowAutomator2.Test>().Test3();
            //await sp.Resolve<WindMouseMover.Test>().TestMove();


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
