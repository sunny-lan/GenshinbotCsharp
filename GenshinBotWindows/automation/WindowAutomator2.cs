
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

namespace genshinbot.automation.windows
{
    public class WindowAutomator2 : IWindowAutomator2
    {

        private HWND hWnd;
        private uint pid, thread;



        public IKeySimulator Keys => throw new NotImplementedException();

        public IMouseSimulator Mouse => throw new NotImplementedException();

        public ScreenshotObservable Screen => throw new NotImplementedException();

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

            var rawLocationStream = locationChangeHook
                    .MergeNotification(closed)
                    .Where(e => e.hwnd == hWnd && e.idObject == User32.ObjectIdentifiers.OBJID_WINDOW)
                    .Select(e => GetRectDirect());

            var locationStream = Observable
                .Return(GetRectDirect())
                .Concat(rawLocationStream)
                    .DistinctUntilChanged();


            foregroundChangeHook = new WinEventHook(
                eventRangeMin: User32.EventConstants.EVENT_SYSTEM_FOREGROUND,
                eventRangeMax: User32.EventConstants.EVENT_SYSTEM_FOREGROUND
            );

            var rawForegroundStream = foregroundChangeHook
                    .MergeNotification(closed)
                    .Where(e => e.idObject == User32.ObjectIdentifiers.OBJID_WINDOW)
                    .Select(e => IsForegroundWindow());

            var foregroundStream = Observable
                .Return(IsForegroundWindow())
                .Concat(rawForegroundStream)
                .DistinctUntilChanged();


            var f = Observable
                .CombineLatest(foregroundStream, locationStream, 
                    (fore, location) => fore && location.Width > 0 && location.Height > 0)
                .DistinctUntilChanged()
                .Replay(1);
            Focused = f;
            disposeList.Add(f.Connect());


            foregroundChangeHook.Start();
            locationChangeHook.Start();
        }

        ~WindowAutomator2()
        {
            #region Focus
            foregroundChangeHook.Stop();
            #endregion

            closed.OnCompleted();

            foreach (var d in disposeList)
                d.Dispose();

        }

        #region Rect

        public IObservable<Size> Size => throw new NotImplementedException();

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


        #endregion

        #region Focus
        private WinEventHook foregroundChangeHook;



        bool IsForegroundWindow()
        {
            return User32.GetForegroundWindow() == hWnd;
        }


        #endregion
        #region Tests
        public static void Test()
        {
            var w = new WindowAutomator2("*Untitled - Notepad", null);
            using (w.Focused.Subscribe(x =>
            Console.WriteLine(x)))
            //using (w.Bounds.Subscribe(r => Console.WriteLine(r)))
            {
                Console.ReadLine();
            }
        }
        #endregion
    }
}
