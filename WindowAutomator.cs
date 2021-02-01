
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;
using Vanara.PInvoke;
using System.ComponentModel;

namespace GenshinbotCsharp
{

    class WindowAutomator :  IHooker
    {




        private HWND hWnd;
        public InputSimulator Simulator;

        public WindowAutomator(string TITLE, string CLASS )
        {
            Screenshot.Init();
            hWnd = User32.FindWindow(CLASS,TITLE);
              if (hWnd == IntPtr.Zero)
                  throw new Exception("failed to find window");

            Simulator = new InputSimulator();


           
        }

        #region Hooking
        private BetterHooker hooker;
        public KeyboardStateTracker KbdState = new KeyboardStateTracker();
        public void InitHooking()
        {
            hooker = new BetterHooker();

        }

        /// <summary>
        /// Waits for the next keyboard/mouse event which the target window recieves
        /// </summary>
        /// <returns></returns>
        public Event WaitEvent()
        {
            if (hooker == null) 
                throw new Exception("InitHooking must be called before using WaitEvent");
            while (true)
            {
                var evt = hooker.WaitEvent();
                if (Focused)
                {
                    if (evt is KeyboardEvent ke)
                        KbdState.OnEvent(ke);
                    return evt;
                }
            }
        }

        public KeyboardEvent WaitKeyboardEvent()
        {
            while (true)
            {
                if (WaitEvent() is KeyboardEvent ke)
                    return ke;
            }
        }        

        public void WaitKeyCombo(params VirtualKeyCode[] combo)
        {
        begin:
            WaitKeyboardEvent();
            foreach (var key in combo)
                if (!KbdState.IsDown(key))
                    goto begin;
        }

        #endregion


        public RECT GetRect()
        {
            RECT r;
            if (!User32.GetClientRect(hWnd, out r))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            Point p = new Point(r.left,r.top);
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
            if (!Focused)
                throw new Exception("genshin not in focus");
            var r = GetRect();


            if (x + img.Mat.Width > r.Width || y + img.Mat.Height > r.Height)
                throw new Exception("screenshot must be within genshin window");

            Point p = new Point(x, y);

            User32.ClientToScreen(hWnd, ref p);

            Screenshot.Take(p.X,p.Y, img);

        }

        public Color GetPixelColor(int x, int y)
        {
            Point p = new Point(x, y);

            User32.ClientToScreen(hWnd, ref p);
            return Screenshot.GetPixelColor(p.X, p.Y);
        }

        #endregion


        #region Focus
        public bool Focused => User32.GetForegroundWindow() == hWnd;

        public void Focus()
        {
            if (!User32.SetForegroundWindow(hWnd))
            {
                throw new Exception("failed to focus window");
            }

        }

        public async Task WaitForFocus()
        {
            while (!Focused)
            {
                await Task.Delay(500);
            }

        }

        public async Task<bool> WaitForFocus(int timeout)
        {
            int time = 0;
            while (!Focused && time < timeout)
            {
                await Task.Delay(500);
                time += 500;
            }
            return Focused;
        }

        #endregion
        public void MouseMove(int dx, int dy)
        {
            if (!Focused)
                throw new Exception("genshin not in focus");
            Simulator.Mouse.MoveMouseBy(dx, dy);
        }



    }
}
