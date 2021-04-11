
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

namespace genshinbot
{
    public class WindowAutomator : IInputSimulator, IWindowAutomator
    {

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
                throw new AttachWindowFailedException();
            constructor(hWnd);

        }
        //TODO attachment

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
                
               Thread.Sleep(1);
            }
        }


        #region Screenshot 

        Point clientToScreen(Point p)
        {
            var pp = new System.Drawing.Point(p.X, p.Y);
            if (!User32.ClientToScreen(hWnd, ref pp))
                throw new Exception();
            return pp.Cv();
        }

        Screenshot.Buffer wholeBuf;


        public Mat Screenshot(OpenCvSharp.Rect r)
        {
            var windowSz = GetSize();
            if (wholeBuf == null || wholeBuf.Size != windowSz)
                wholeBuf = genshinbot.Screenshot.GetBuffer(windowSz.Width, windowSz.Height);
            genshinbot.Screenshot.Take(wholeBuf, r.Size, clientToScreen(r.TopLeft), r.TopLeft);
            return wholeBuf.Mat[r];
        }

        /// <summary>
        /// screenshots rectangle starting at (x,y) in the client area into img
        /// size of rectangle defined by size of img
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="img"></param>
       /* public void TakeScreenshot(int x, int y, Screenshot.Buffer img)
        {
            WaitForFocus();

            var r = GetRect();


            if (x + img.Mat.Width > r.Width || y + img.Mat.Height > r.Height)
                throw new Exception("screenshot must be within genshin window");

            var p = new System.Drawing.Point(x, y);

            User32.ClientToScreen(hWnd, ref p);

            Screenshot.Take(p.X, p.Y, img);

        }*/

        public Scalar GetPixelColor(int x, int y)
        {
            return Screenshot(new Rect(x, y, 1, 1)).Get<Vec3b>(0, 0).Cvt();
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
            if(foregroundChangeHook!=null)
            foregroundChangeHook.Stop();
        }



        #endregion


        #region Rect
        public event EventHandler<Rect> OnClientAreaChanged;

        WinEventHook locationChangeHook;
        private Rect rect;
        private InputSimulatorStandard.InputSimulator iS;

        Rect GetRect()
        {
            //Waiting for focus guarentees rect is intialized, 
            //as it will always fetch the rect as soon as the window recieves focus
            WaitForFocus();
            return rect;
        }

        public Size GetSize()
        {
            return GetRect().Size;
        }

        /// <summary>
        /// returns the bounds of the client area
        /// can assume x,y will always be 0
        /// and size will be the size of the client area
        /// </summary>
        /// <returns></returns>
        public Rect GetBounds()
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
            if(locationChangeHook!=null)
            locationChangeHook.Stop();
        }

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

        #region InputSimulator

        private void initInput()
        {
            iS = new InputSimulatorStandard.InputSimulator();
        }
        public OpenCvSharp.Point2d MousePos()
        {
            WaitForFocus();
            var pt = Cursor.Position;
            if (!User32.ScreenToClient(hWnd, ref pt))
                throw new Exception();
            return pt.Cv();
        }

        public void SendMouseInput(User32.MOUSEINPUT mi)
        {
            SendInput(new User32.INPUT
            {
                type = User32.INPUTTYPE.INPUT_MOUSE,
                mi = mi,
            });
        }

        public void MouseMove(OpenCvSharp.Point2d p)
        {
            WaitForFocus();
            var oo = p.Round();
            iS.Mouse.MoveMouseBy(oo.X, oo.Y);
            return;
            var cur = MousePos();
            MouseTo(cur + p);

            /*Point pp = new Point((int)Math.Round(p.X), (int)Math.Round(p.Y));
            cvtPixelToMouse(ref pp);
            SendMouseInput(new User32.MOUSEINPUT
            {
                dx = pp.X,
                dy = pp.Y,
                dwFlags = (uint)User32.MOUSEEVENTF.MOUSEEVENTF_MOVE
            });*/

        }

        void cvtPixelToMouse(ref Point p)
        {
            User32.GetClientRect(User32.GetDesktopWindow(), out var desktop);
            p.X = 65536 * p.X / desktop.right;
            p.Y = 65536 * p.Y / desktop.bottom;
        }

        public void MouseTo(Point2d p)
        {
            WaitForFocus();
            var pp = new System.Drawing.Point((int)Math.Round(p.X), (int)Math.Round(p.Y));

            if (!User32.ClientToScreen(hWnd, ref pp))
                throw new Exception();

            //cvtPixelToMouse(ref pp);

            /*SendMouseInput(new User32.MOUSEINPUT
            {
                dx = pp.X,
                dy = pp.Y,
                dwFlags = (uint)User32.MOUSEEVENTF.MOUSEEVENTF_MOVE | (uint)User32.MOUSEEVENTF.MOUSEEVENTF_ABSOLUTE |(uint)User32.MOUSEEVENTF.MOUSEEVENTF_VIRTUALDESK
            });*/
           iS.Mouse.MoveMouseToPositionOnVirtualDesktop(pp.X,pp.Y);
        }

        static readonly User32.MOUSEEVENTF[] mouseDownF = {
            User32.MOUSEEVENTF.MOUSEEVENTF_LEFTDOWN,
            User32.MOUSEEVENTF.MOUSEEVENTF_MIDDLEDOWN,
            User32.MOUSEEVENTF.MOUSEEVENTF_RIGHTDOWN,
        };
        static readonly User32.MOUSEEVENTF[] mouseUpF = {
            User32.MOUSEEVENTF.MOUSEEVENTF_LEFTUP,
            User32.MOUSEEVENTF.MOUSEEVENTF_MIDDLEUP,
            User32.MOUSEEVENTF.MOUSEEVENTF_RIGHTUP,
        };

        static uint CvtBtn(int btn, bool down)
        {
            var x = (down ? mouseDownF : mouseUpF);
            return (uint)x[btn];
        }

        public void MouseButton(int btn, bool down)
        {
            SendMouseInput(new User32.MOUSEINPUT
            {
                dwFlags = CvtBtn(btn, down),
            });
        }

        public void MouseDown(int btn)
        {
            WaitForFocus();
            if (btn == 0)
                iS.Mouse.LeftButtonDown();
            else throw new NotImplementedException();
        }

        public void MouseUp(int btn)
        {
            WaitForFocus();
            if (btn == 0)
                iS.Mouse.LeftButtonUp();
            else throw new NotImplementedException();
        }

        public void MouseClick(int btn)
        {
            MouseDown(btn);
            Thread.Sleep(100);
            MouseUp(btn);
        }



        public void SendInput(User32.INPUT input)
        {
            WaitForFocus();
            if(User32.SendInput(1, new User32.INPUT[] {
               input
            }, Marshal.SizeOf<User32.INPUT>()) == 0){
                throw Kernel32.GetLastError().GetException();
            }
        }

        public void SendKeyEvent(int k, bool down)
        {
            SendInput(new User32.INPUT
            {
                type = User32.INPUTTYPE.INPUT_KEYBOARD,
                ki = new User32.KEYBDINPUT
                {
                    wVk = (ushort)k,
                    dwFlags = down ? default : User32.KEYEVENTF.KEYEVENTF_KEYUP,
                }
            });
        }

        public void KeyDown(int k)
        {
            WaitForFocus();
            iS.Keyboard.KeyDown((InputSimulatorStandard.Native.VirtualKeyCode)k);
        }

        public void KeyUp(int k)
        {
            WaitForFocus();
            iS.Keyboard.KeyUp((InputSimulatorStandard.Native.VirtualKeyCode)k);
        }

        public void KeyPress(int k)
        {
            KeyDown(k);
            Thread.Sleep(100);
            KeyUp(k);
        }

        public void MouseButton(OpenCvSharp.Point2d pos, int btn, bool down)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
