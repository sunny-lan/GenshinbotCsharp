
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

namespace genshinbot.automation.windows
{
    public class WindowAutomator2 : IWindowAutomator2
    {

        private HWND hWnd;
        private uint pid, thread;



        /// <summary>
        /// Outputs rects only when the window is focused
        /// </summary>
        private IObservable<Rect> clientAreaFocused;



        public void TryFocus()
        {
            if (!User32.SetForegroundWindow(hWnd))
            {
                throw new Exception("failed to focus window");
            }
        }

        public IObservable<bool> Focused { get; private init; }

        private Subject<Unit> closed;
        private List<IDisposable> disposeList = new List<IDisposable>();
        public WindowAutomator2(string TITLE, string CLASS)
        {
            hWnd = User32.FindWindow(CLASS, TITLE);
            if (hWnd == IntPtr.Zero)
                throw new AttachWindowFailedException();
            thread = User32.GetWindowThreadProcessId(hWnd, out pid);
            closed = new Subject<Unit>();

            locationChangeHook = new WinEventHook(processOfInterest: pid);

            var rawClientAreaStream = locationChangeHook
                .MergeNotification(closed)
                .Where(e => e.hwnd == hWnd && e.idObject == User32.ObjectIdentifiers.OBJID_WINDOW)
                .Select(e => GetRectDirect());
            locationChangeHook.Start();

            clientAreaStream = Observable
                .Return(GetRectDirect())
                .Concat(rawClientAreaStream)
                .DistinctUntilChanged()
                .Replay(1);
            disposeList.Add(clientAreaStream.Connect());


            foregroundChangeHook = new WinEventHook(
                eventRangeMin: User32.EventConstants.EVENT_SYSTEM_FOREGROUND,
                eventRangeMax: User32.EventConstants.EVENT_SYSTEM_FOREGROUND
            );

            var rawForegroundStream = foregroundChangeHook
                    .MergeNotification(closed)
                    .Where(e => e.idObject == User32.ObjectIdentifiers.OBJID_WINDOW)
                    .Select(e => IsForegroundWindow());
            foregroundChangeHook.Start();

            var foregroundStream = Observable
                .Return(IsForegroundWindow())
                .Concat(rawForegroundStream)
                .DistinctUntilChanged();


            var f_tmp = Observable
                .CombineLatest(foregroundStream, clientAreaStream,
                    (foreground, clientArea) => foreground && clientArea.Width > 0 && clientArea.Height > 0)
                .DistinctUntilChanged()
                .Replay(1);
            Focused = f_tmp;
            disposeList.Add(f_tmp.Connect());

            var s_tmp = clientAreaStream
                .Select(r => r.Size)
                .Where(s => s.Width > 0 && s.Height > 0)
                .DistinctUntilChanged()
                .Replay(1);
            Size = s_tmp;
            disposeList.Add(s_tmp.Connect());

            clientAreaFocused = clientAreaStream
                .Relay(Focused)
                .DistinctUntilChanged();

            Screen = new ScreenshotAdapter(this);
            iS = new InputSim(this);

        }

        ~WindowAutomator2()
        {
            foregroundChangeHook.Stop();
            locationChangeHook.Stop();

            closed.OnCompleted();

            foreach (var d in disposeList)
                d.Dispose();

        }

        #region Rect

        public IObservable<Size> Size { get; private init; }

        private WinEventHook locationChangeHook;

        private Rect GetRectDirect()
        {

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

        private IConnectableObservable<Rect> clientAreaStream;


        #endregion

        #region Focus
        private WinEventHook foregroundChangeHook;



        bool IsForegroundWindow()
        {
            return User32.GetForegroundWindow() == hWnd;
        }


        #endregion


        #region Input

        public IKeySimulator2 Keys => iS;
        public IMouseSimulator2 Mouse => iS;
        private InputSim iS;
        class InputSim : IMouseSimulator2, IKeySimulator2
        {
            private InputSimulatorStandard.InputSimulator iS;
            WindowAutomator2 parent;

            internal InputSim(WindowAutomator2 parent)
            {

                iS = new InputSimulatorStandard.InputSimulator();
                this.parent = parent;
            }


            ~InputSim()
            {

            }




            public Task Key(input.Keys k, bool down)
            {
                void _Key()
                {
                    if (down)
                    {
                        iS.Keyboard.KeyDown((InputSimulatorStandard.Native.VirtualKeyCode)k);
                    }
                    else
                    {
                        iS.Keyboard.KeyUp((InputSimulatorStandard.Native.VirtualKeyCode)k);
                    }
                }
                return parent.Focused.LockWhile(() => Task.Run(_Key));
            }

            public Task MouseButton(MouseBtn btn, bool down)
            {
                void _mouse()
                {
                    if (down)
                    {
                        if (btn == MouseBtn.Left) iS.Mouse.LeftButtonDown();
                        else if (btn == MouseBtn.Right) iS.Mouse.RightButtonDown();
                        else iS.Mouse.MiddleButtonDown();
                    }
                    else
                    {
                        if (btn == MouseBtn.Left) iS.Mouse.LeftButtonUp();
                        else if (btn == MouseBtn.Right) iS.Mouse.RightButtonUp();
                        else iS.Mouse.MiddleButtonUp();
                    }
                }
                return parent.Focused.LockWhile(() => Task.Run(_mouse));
            }

            public Task MouseMove(Point2d d)
            {
                void _mouse()
                {
                    DPIAware.Use(DPIAware.DPI_AWARENESS_CONTEXT_UNAWARE, () =>
                    {
                        var oo = d.Round();
                        iS.Mouse.MoveMouseBy(oo.X, oo.Y);
                    });
                }
                return parent.Focused.LockWhile(() => Task.Run(_mouse));

            }

            public Task<Point2d> MousePos()
            {
                Point2d _mouse()
                {
                    return DPIAware.Use(DPIAware.DPI_AWARENESS_CONTEXT_UNAWARE, () =>
                    {
                        var pt = Cursor.Position;
                        if (!User32.ScreenToClient(parent.hWnd, ref pt))
                            throw new Exception();
                        return pt.Cv();
                    });
                }
                return parent.Focused.LockWhile(() => Task.Run(_mouse));
            }

            void cvtPixelToMouse(ref System.Drawing.Point p)
            {
                // User32.GetClientRect(User32.GetDesktopWindow(), out var desktop);
                p.X = 65536 * p.X / SystemInformation.VirtualScreen.Width;
                p.Y = 65536 * p.Y / SystemInformation.VirtualScreen.Height;
            }

            public Task MouseTo(Point2d p)
            {
                void _MouseTo()
                {
                    var pp = new System.Drawing.Point((int)Math.Round(p.X), (int)Math.Round(p.Y));
                    DPIAware.Use(DPIAware.DPI_AWARENESS_CONTEXT_UNAWARE, () =>
                    {
                        if (!User32.ClientToScreen(parent.hWnd, ref pp))
                            throw new Exception();

                        cvtPixelToMouse(ref pp);

                        /*SendMouseInput(new User32.MOUSEINPUT
                        {
                            dx = pp.X,
                            dy = pp.Y,
                            dwFlags = (uint)User32.MOUSEEVENTF.MOUSEEVENTF_MOVE | (uint)User32.MOUSEEVENTF.MOUSEEVENTF_ABSOLUTE |(uint)User32.MOUSEEVENTF.MOUSEEVENTF_VIRTUALDESK
                        });*/
                        iS.Mouse.MoveMouseToPositionOnVirtualDesktop(pp.X, pp.Y);
                    });
                }

                return parent.Focused.LockWhile(() => Task.Run(_MouseTo));
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
        class ScreenshotAdapter : ScreenshotObservable
        {
            WindowAutomator2 parent;
            private GDIStream gdi;

            public ScreenshotAdapter(WindowAutomator2 parent)
            {
                this.parent = parent;
                gdi = new GDIStream(parent.Focused);
            }

            public IObservable<Mat> Watch(Rect r)
            {
                return parent.clientAreaFocused
                    .Select(clientArea =>
                    {
                        Rect actual = new Rect(parent.clientToScreen(r.TopLeft), r.Size);
                        return gdi.Watch(actual);
                    })
                    .Switch();
            }
        }
        #endregion

        #region Tests
        public static void Test()
        {
            var w = new WindowAutomator2("*Untitled - Notepad", null);
            using (w.Focused.Subscribe(x => Console.WriteLine(x)))
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
                using (w.Focused.Subscribe(x => Console.WriteLine($"focused={x}")))
                {
                    /*for (int i = 0; i < 10; i++)
                    {
                        await w.Keys.KeyPress(input.Keys.A);
                        await Task.Delay(100);

                    }*/
                    var sz = await w.Size.Get();
                    Console.WriteLine($"sz={sz}");
                    for (int i = 0; i < 100000; i++)
                    {
                        await w.Mouse.MouseTo(sz.Bounds().RandomWithin());
                    }
                    await w.Mouse.MouseTo(sz.Bounds().BottomRight);
                }
                Console.ReadLine();
            }

        }

        public static  void Test3()
        {
            var w = new WindowAutomator2("*Untitled - Notepad", null);
            using (w.Screen.Watch(new Rect(10,10,100,100)).Subscribe(m =>
            {
                CvThread.ImShow("m", m);
            }))
            {
                Console.ReadLine();
            }
        }
        #endregion
    }
}
