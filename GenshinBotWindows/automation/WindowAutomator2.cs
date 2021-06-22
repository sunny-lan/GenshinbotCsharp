
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Vanara.PInvoke;
using System.ComponentModel;
using genshinbot.hooks;
using genshinbot.util;
using OpenCvSharp;
using genshinbot.automation;
using genshinbot.automation.input;
using System.Windows.Forms;
using genshinbot.reactive;
using genshinbot.automation.screenshot;
using System.Reactive.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Collections.Generic;
using System.Threading.Tasks;
using genshinbot.automation.screenshot.gdi;
using genshinbot.diag;
using genshinbot.automation.hooking;
using genshinbot.reactive.wire;
using genshinbot.automation.input.windows;

namespace genshinbot.automation.windows
{
    public class WindowAutomator2 : IWindowAutomator2
    {

        private HWND hWnd;
        private uint pid, thread;





        public void TryFocus()
        {
            if (!User32.SetForegroundWindow(hWnd))
            {
                throw new Exception("failed to focus window");
            }
        }

        public ILiveWire<bool> Focused { get; private init; }

        private List<IDisposable> disposeList = new List<IDisposable>();
        public WindowAutomator2(string TITLE, string CLASS)
        {
            hWnd = User32.FindWindow(CLASS, TITLE);
            if (hWnd == IntPtr.Zero)
                throw new AttachWindowFailedException();
            thread = User32.GetWindowThreadProcessId(hWnd, out pid);

            locationChangeHook = new WinEventHook(processOfInterest: pid);

            clientAreaStream = locationChangeHook.Wire

                .Where(e => e.hwnd == hWnd && e.idObject == User32.ObjectIdentifiers.OBJID_WINDOW)
                //.Do(e=>Console.WriteLine($"e={e.idObject} {e.hwnd.DangerousGetHandle()}"))
                .ToLive(() => GetRectDirect())
                .DistinctUntilChanged()
                .Debug("clientArea")
                ;
            locationChangeHook.Start();



            foregroundChangeHook = new WinEventHook(
                eventRangeMin: User32.EventConstants.EVENT_SYSTEM_FOREGROUND,
                eventRangeMax: User32.EventConstants.EVENT_SYSTEM_FOREGROUND
            );

            //TODO some weird issue with this stream not working
            // TODO some sketchy reason, foregroundChangehook doesn't work all the time
            var foregroundStream = Wire.Merge<NoneT>(
                    foregroundChangeHook.Wire
                        .Where(
                        e => 
                        e.idObject == User32.ObjectIdentifiers.OBJID_WINDOW).Nonify()
                        ,
                        clientAreaStream.Nonify()
                    )
                    .Debug("RAW foreground")
                    .ToLive(() => IsForegroundWindow())
                    .Debug("LIVE foreground")
                    .DistinctUntilChanged()
                 .Debug("foreground");
            ;
            foregroundChangeHook.Start();
            //perform merged processing path
            var combined = Wire.Combine(foregroundStream, clientAreaStream, (foreground, clientArea) =>
            {
                var focused = foreground && clientArea.Width > 0 && clientArea.Height > 0;
                Rect? screenBounds = focused ? clientArea : null;
                return (focused, screenBounds);
            }).Debug("combined");

            //split after merged
            Focused = combined.Select(x => x.focused)
               .DistinctUntilChanged()
                .Debug("focused");
            ;

            ScreenBounds = combined.Select(x => x.screenBounds)
                .DistinctUntilChanged()
                 .Debug("screenbounds");
            ;

            Size = ScreenBounds
                .Select(r => r?.Size)
                .DistinctUntilChanged();

            Bounds = Size.Select(x => x?.Bounds())
                            .DistinctUntilChanged()
                ;



            Screen = new ScreenshotAdapter(this);
            iS = new InputSim(this);
            mouseCap = new Lazy<MouseHookAdapter>(() => new MouseHookAdapter(Focused,
                pt => ScreenToClient(pt)));
            keyCap = new Lazy<KbdHookAdapter>(() => new KbdHookAdapter(Focused));

        }

        ~WindowAutomator2()
        {
            foregroundChangeHook.Stop();
            locationChangeHook.Stop();

            foreach (var d in disposeList)
                d.Dispose();

        }

        #region Rect

        public ILiveWire<Size?> Size { get; private init; }
        public ILiveWire<Rect?> Bounds { get; private init; }

        private WinEventHook locationChangeHook;

        private Rect GetRectDirect()
        {
            //Console.WriteLine("GetRect");
            RECT r;
            if (!User32.GetClientRect(hWnd, out r))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            var p = new System.Drawing.Point(r.left, r.top);
            User32.ClientToScreen(hWnd, ref p);
            r.left = p.X;
            r.top = p.Y;

            p.X = r.right;
            p.Y = r.bottom;
            User32.ClientToScreen(hWnd, ref p);
            r.right = p.X;
            r.bottom = p.Y;

            return r.Cv();
        }

        private ILiveWire<Rect> clientAreaStream;


        #endregion

        #region Focus
        private WinEventHook foregroundChangeHook;



        bool IsForegroundWindow()
        {
            //Console.WriteLine("IsFore");
            return User32.GetForegroundWindow() == hWnd;
        }


        #endregion


        #region Input

        public IKeySimulator2 Keys => iS;
        public IMouseSimulator2 Mouse => iS;
        private InputSim iS;
        class InputSim : IMouseSimulator2, IKeySimulator2
        {
            IMouseSimulator2 ms;
            IKeySimulator2 ks;
            WindowAutomator2 parent;

            internal InputSim(WindowAutomator2 parent)
            {
                var a = new InputSimulatorStandardAdapter();
                ms = a;
                ks = a;
                this.parent = parent;
            }


            ~InputSim()
            {

            }




            public Task Key(input.Keys k, bool down)
            {

                return parent.Focused.LockWhile(() => ks.Key(k, down));
            }

            public Task MouseButton(MouseBtn btn, bool down)
            {

                return parent.Focused.LockWhile(() => ms.MouseButton(btn, down));
            }

            public Task MouseMove(Point2d d)
            {
                return parent.Focused.LockWhile(() => ms.MouseMove(d));

            }

            public Task<Point2d> MousePos()
            {
                return parent.Focused.LockWhile(ms.MousePos);
            }

            void cvtPixelToMouse(ref System.Drawing.Point p)
            {
                // User32.GetClientRect(User32.GetDesktopWindow(), out var desktop);
                p.X = 65536 * p.X / SystemInformation.VirtualScreen.Width;
                p.Y = 65536 * p.Y / SystemInformation.VirtualScreen.Height;
            }

            public Task MouseTo(Point2d p)
            {
                Task _MouseTo()
                {
                    var pp = new System.Drawing.Point((int)Math.Round(p.X), (int)Math.Round(p.Y));
                    DPIAware.Use(DPIAware.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE, () =>
                    {
                        if (!User32.ClientToScreen(parent.hWnd, ref pp))
                            throw new Exception();

                        /*SendMouseInput(new User32.MOUSEINPUT
                        {
                            dx = pp.X,
                            dy = pp.Y,
                            dwFlags = (uint)User32.MOUSEEVENTF.MOUSEEVENTF_MOVE | (uint)User32.MOUSEEVENTF.MOUSEEVENTF_ABSOLUTE |(uint)User32.MOUSEEVENTF.MOUSEEVENTF_VIRTUALDESK
                        });*/
                    });
                    return ms.MouseTo(pp.Cv());
                }

                return parent.Focused.LockWhile(_MouseTo);
            }
        }
        #endregion

        #region Screenshot
        Point clientToScreen(Point p)
        {
            var pp = new System.Drawing.Point(p.X, p.Y);
            if (!User32.ClientToScreen(hWnd, ref pp))
                throw new Exception();
            return pp.Cv();
        }

        public ScreenshotObservable Screen { get; private init; }

        public IMouseCapture MouseCap => mouseCap.Value;
        private Lazy<MouseHookAdapter> mouseCap;
        public IKeyCapture KeyCap => keyCap.Value;

        public ILiveWire<Rect?> ScreenBounds { get; private init; }

        private Lazy<KbdHookAdapter> keyCap;

        class ScreenshotAdapter : ScreenshotObservable
        {
            WindowAutomator2 parent;
            private ScreenshotObservable gdi;

            public ScreenshotAdapter(WindowAutomator2 parent)
            {
                this.parent = parent;
                gdi = new GDIStream(parent.Focused);
            }

            public IWire<Pkt<Mat>> Watch(Rect r)
            {
                var mappedRectStream = parent.ScreenBounds
                    .Select(_ => DPIAware.Use(DPIAware.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE, () =>
                     {
                         return new Rect(parent.clientToScreen(r.TopLeft), r.Size);
                     }));
                return gdi.Watch(mappedRectStream);
            }
        }
        #endregion

        #region Tests
        public static void Test()
        {
            var w = new WindowAutomator2("*Untitled - Notepad", null);
            using (w.Focused.Connect(x => Console.WriteLine(x)))
            using (w.Size.Subscribe(r => Console.WriteLine(r)))
            {
                for (int i = 0; i < 3; i++)
                {
                    Console.ReadLine();
                    var t = w.Size.Get();
                    t.Wait();
                    Console.WriteLine($" get={t.Result}");
                    using (w.Size.Subscribe(r => Console.WriteLine($" ss={r}"))) { }
                    using (w.Focused.Subscribe(r => Console.WriteLine($" ff={r}"))) { }
                }
                Console.ReadLine();
            }
            Console.WriteLine("waiting for focus, timeout=100");
            try
            {
                (w as IWindowAutomator2).WaitForFocus(TimeSpan.FromMilliseconds(100)).Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine($"caught {e}");
            }
            Console.ReadLine();
            Console.WriteLine("waiting for focus");
            (w as IWindowAutomator2).WaitForFocus().Wait();


        }

        public static async Task Test2()
        {
            var w = new WindowAutomator2("*Untitled - Notepad", null);
            while (true)
            {
                Console.ReadLine();
                using (w.Focused.Subscribe(x => Console.WriteLine($"focused={x}")))
                {
                    /*for (int i = 0; i < 10; i++)
                    {
                        await w.Keys.KeyPress(input.Keys.A);
                        await Task.Delay(100);

                    }*/
                    var sz = await w.Size.Value2();
                    Console.WriteLine($"sz={sz}");
                    for (int i = 0; i < 1000; i++)
                    {
                        await w.Mouse.MouseTo(sz.Bounds().RandomWithin());
                    }
                    await w.Mouse.MouseTo(sz.Bounds().BottomRight);
                }
                Console.ReadLine();
            }

        }

        public static void Test3()
        {
            IWindowAutomator2 w = new WindowAutomator2("*Untitled - Notepad", null);
            /*Console.WriteLine("fixed");
           using (w.Screen.Watch(new Rect(10, 10, 100, 100)).Subscribe(m =>
               {
                   CvThread.ImShow("m", m);
               }))
            {
                Console.ReadLine();
            }*/

            Console.WriteLine("follow screen sz");
            using (w.Screen.Watch2(w.Bounds).Subscribe(m =>
            {
                CvThread.ImShow("m", m.Value);
            }))
            {
                Console.ReadLine();
            }
        }

        public static void Test4()
        {
            IWindowAutomator2 w = new WindowAutomator2("*Untitled - Notepad", null);
            using (w.MouseCap.MouseEvents.Subscribe(x =>
                 Console.WriteLine(x.Position)
                ))
            using (w.KeyCap.KeyEvents.Subscribe(
                x => Console.WriteLine($"{x.Key} {x.Down}")
                ))
            using (w.KeyCap.KbdState.KeyCombo(new input.Keys[] {
                input.Keys.LControlKey,
                input.Keys.B,
            }).Subscribe(
                x => Console.WriteLine($"key combo {x}")
                ))
            {
                Console.ReadLine();
            }
        }

        public Point ScreenToClient(Point p)
        {
            //TODO
            var pt = p.Sys();
            User32.ScreenToClient(hWnd, ref pt);
            return pt.Cv();
        }
        public Point ClientToScreen(Point p)
        {
            //TODO
            var pt = p.Sys();
            User32.ClientToScreen(hWnd, ref pt);
            return pt.Cv();
        }
        #endregion
    }
}
