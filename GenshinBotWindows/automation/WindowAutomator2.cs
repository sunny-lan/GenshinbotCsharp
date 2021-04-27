
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

namespace genshinbot.automation.windows
{
    public class WindowAutomator2 : IWindowAutomator2
    {

        private HWND hWnd;
        private uint pid, thread;

        

        public IKeySimulator2 Keys => iS;

        public IMouseSimulator2 Mouse => iS;

        public ScreenshotObservable Screen { get; private init; }

        private InputSim iS;

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

            Screen = new screenshot.gdi.GDIStream();
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
        class InputSim : IMouseSimulator2, IKeySimulator2
        {
            private InputSimulatorStandard.InputSimulator iS;
            IWindowAutomator2 parent;

            internal InputSim(IWindowAutomator2 parent)
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
                return parent.Focused.LockWhile(()=>Task.Run(_Key));
            }

            public Task MouseButton(MouseBtn btn, bool down)
            {
                throw new NotImplementedException();
            }

            public Task MouseMove(Point2d d)
            {
                throw new NotImplementedException();
            }

            public Task<Point2d> MousePos()
            {
                throw new NotImplementedException();
            }

            public Task MouseTo(Point2d p)
            {
                throw new NotImplementedException();
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
                for (int i = 0; i < 3;i++)
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
            catch(Exception e)
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
                    for (int i = 0; i < 10; i++)
                    {
                        Console.WriteLine("trigger key");
                        await w.Keys.KeyPress(input.Keys.A);
                        await Task.Delay(100);

                    }
                }
                Console.ReadLine();
            }

        }
        #endregion
    }
}
