
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput.Native;
using Vanara.PInvoke;
using System.ComponentModel;
using GenshinbotCsharp.hooks;
using GenshinbotCsharp.util;

namespace GenshinbotCsharp
{

    class WindowAutomator:input.IInputSimulator
    {
        public input.IInputSimulator I;

        

        private HWND hWnd;
        private uint pid, thread;

        public WindowAutomator(HWND hwnd)
        {
            constructor(hwnd);
        }

        private void constructor(HWND hwnd)
        {
            if (hwnd.IsNull) throw new Exception("invalid hwnd");
            this.hWnd = hwnd;
            thread = User32.GetWindowThreadProcessId(hWnd, out pid);

            initInput();
            initFocus();
            initRect();


        }
        public WindowAutomator(string TITLE, string CLASS)
        {
            hWnd = User32.FindWindow(CLASS, TITLE);
            if (hWnd == IntPtr.Zero)
                throw new Exception("failed to find window");
            constructor(hWnd);
            
        }

        ~WindowAutomator()
        {
            cleanupFocus();
            cleanupRect();
        }


        public static void Test()
        {
            //var w = new WindowAutomator("*Untitled - Notepad", null);
            var w = GenshinWindow.FindExisting();
            w.WaitForFocus();
            w.OnClientAreaChanged += (_, r) => Console.WriteLine("changed " + r);
            w.OnFocusChanged += (_, f) => Console.WriteLine("focused " + f);
            while (true)
            {
                var r = w.GetRect();
                var b = Screenshot.GetBuffer(r.Width, r.Height);
                while (w.GetRect().Size == r.Size)
                {
                    Thread.Sleep(1);
                    //w.TakeScreenshot(0, 0, b);
                    // Debug.img = b.Mat;
                    // Debug.show();
                }
            }
        }


        #region Screenshot 


        /// <summary>
        /// screenshots rectangle starting at (x,y) in the client area into img
        /// size of rectangle defined by size of img
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="img"></param>
        public void TakeScreenshot(int x, int y, Screenshot.Buffer img)
        {
            WaitForFocus();

            var r = GetRect();


            if (x + img.Mat.Width > r.Width || y + img.Mat.Height > r.Height)
                throw new Exception("screenshot must be within genshin window");

            Point p = new Point(x, y);

            User32.ClientToScreen(hWnd, ref p);

            Screenshot.Take(p.X, p.Y, img);

        }

        public Color GetPixelColor(int x, int y)
        {
            WaitForFocus();

            Point p = new Point(x, y);

            User32.ClientToScreen(hWnd, ref p);
            return Screenshot.GetPixelColor(p.X, p.Y);
        }

        #endregion


        #region Focus
        public event EventHandler<bool> OnFocusChanged;
        
        WinEventHook foregroundChangeHook;
        EventWaiter<bool> focusChangeWaiter;
        private bool foreground;

        public bool Focused { get; private set; }

        private void RefreshFocused()
        {
            bool newVal = foreground && rect.Size.Width > 0 && rect.Size.Height > 0;
            if (newVal != Focused)
            {
                Focused = newVal;
                focusChangeWaiter.Signal(newVal);
                OnFocusChanged?.Invoke(this, newVal);
            }
        }

        private void RefreshForeground()
        {
            var x = IsForegroundWindow();
            if (x != foreground)
            {
                foreground = x;
                RefreshFocused();
            }
        }

        private void initFocus()
        {
            focusChangeWaiter = new EventWaiter<bool>();

            foregroundChangeHook = new WinEventHook(
                eventRangeMin: User32.EventConstants.EVENT_SYSTEM_FOREGROUND,
                eventRangeMax: User32.EventConstants.EVENT_SYSTEM_FOREGROUND
            );
            foregroundChangeHook.OnEvent += HandleForegroundWindowChanged;

            RefreshForeground();
            foregroundChangeHook.Start();
        }

        private void HandleForegroundWindowChanged(object sender, WinEvent e)
        {
            if (e.idObject != User32.ObjectIdentifiers.OBJID_WINDOW) return;
            RefreshForeground();
        }

        public void WaitForFocus(int timeout = -1)
        {
            while (!Focused)
                focusChangeWaiter.WaitEvent(timeout);
        }

        public void TryFocus()
        {
            if (!User32.SetForegroundWindow(hWnd))
            {
                throw new Exception("failed to focus window");
            }
        }

        public bool IsForegroundWindow()
        {
            return User32.GetForegroundWindow() == hWnd;
        }

        private void cleanupFocus()
        {
            foregroundChangeHook.Stop();
        }



        #endregion


        #region Rect
        public event EventHandler<RECT> OnClientAreaChanged;

        WinEventHook locationChangeHook;
        private RECT rect;

        public RECT GetRect()
        {
            //Waiting for focus guarentees rect is intialized, 
            //as it will always fetch the rect as soon as the window recieves focus
            WaitForFocus();
            return rect;
        }

        public RECT GetBounds()
        {
            var r = GetRect();
            r.X = 0;
            r.Y = 0;
            return r;
        }

        private void initRect()
        {

            locationChangeHook = new WinEventHook(processOfInterest: pid);
            locationChangeHook.OnEvent += HandleWindowLocationChanged;

            RefreshRect();
            locationChangeHook.Start();
        }

        private void HandleWindowLocationChanged(object sender, WinEvent e)
        {
            if (e.hwnd == hWnd && e.idObject == User32.ObjectIdentifiers.OBJID_WINDOW)
            {
                RefreshRect();
            }
        }

        private void RefreshRect()
        {
            var x = GetRectDirect();
            if (rect != x)
            {
                rect = x;
                RefreshFocused();
                if (Focused)
                    OnClientAreaChanged?.Invoke(this, x);
            }
        }

        private void cleanupRect()
        {
            locationChangeHook.Stop();
        }

        private RECT GetRectDirect()
        {

            RECT r;
            if (!User32.GetClientRect(hWnd, out r))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            Point p = new Point(r.left, r.top);
            User32.ClientToScreen(hWnd, ref p);
            r.left = p.X;
            r.top = p.Y;

            p.X = r.right;
            p.Y = r.bottom;
            User32.ClientToScreen(hWnd, ref p);
            r.right = p.X;
            r.bottom = p.Y;

            return r;
        }

        #endregion

        #region InputSimulator

        private void initInput()
        {
            I = this;
        }
        public OpenCvSharp.Point2d MousePos()
        {
            WaitForFocus();
            throw new NotImplementedException();

        }

        public void MouseMove(OpenCvSharp.Point2d d)
        {
            throw new NotImplementedException();
        }

        public void MouseTo(OpenCvSharp.Point2d p)
        {
            throw new NotImplementedException();
        }

        public void MouseDown(int btn)
        {
            throw new NotImplementedException();
        }

        public void MouseUp(int btn)
        {
            throw new NotImplementedException();
        }

        public void MouseClick(int btn)
        {
            throw new NotImplementedException();
        }

        public void SendInput(User32.INPUT input)
        {
            WaitForFocus();
            User32.SendInput(1, new User32.INPUT[] {
               input
            }, Marshal.SizeOf<User32.INPUT>());
        }

        public void SendKeyEvent(int k, bool down)
        {
            SendInput(new User32.INPUT
            {
                type = User32.INPUTTYPE.INPUT_KEYBOARD,
                ki = new User32.KEYBDINPUT
                {
                    wVk = (ushort)k,
                    dwFlags=down?default:User32.KEYEVENTF.KEYEVENTF_KEYUP,
                }
            });
        }

        public void KeyDown(int k)
        {
            SendKeyEvent(k, true);
        }

        public void KeyUp(int k)
        {
            SendKeyEvent(k, false);
        }

        public void KeyPress(int k)
        {
            KeyDown(k);
            KeyUp(k);
        }
        #endregion
    }
}
