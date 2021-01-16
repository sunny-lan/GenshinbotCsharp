
using HookLib;
using HookLib.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace GenshinbotCsharp
{

    class GenshinWindow
    {




        private volatile IntPtr hWnd;
        public InputSimulator Simulator;
        private IntPtr hdc;
        private Hooker recorder;

        public GenshinWindow()
        {
            hWnd = WinAPI.FindWindow("UnityWndClass", "Genshin Impact");
            if (hWnd == IntPtr.Zero)
                throw new Exception("failed to find window");
            Simulator = new InputSimulator();

            hdc = WinAPI.GetDC(hWnd);
            if (hdc == IntPtr.Zero)
                throw new Exception("failed to get DC (to get pixel colors)");

            recorder = new Hooker();

            //stupid hack
       

        }




        ~GenshinWindow()
        {
            if (WinAPI.ReleaseDC(hWnd, hdc) != 1)
                throw new Exception("unable to release DC");
            
        }

        public bool Focused => WinAPI.GetForegroundWindow() == hWnd;

        public void Focus()
        {
            if (!WinAPI.SetForegroundWindow(hWnd))
            {
                throw new Exception("failed to focus window");
            }

        }

        public Color GetPixelColor(int x, int y)
        {
            uint pixel = WinAPI.GetPixel(hdc, x, y);
            Color color = Color.FromArgb((int)(pixel & 0x000000FF),
                            (int)(pixel & 0x0000FF00) >> 8,
                            (int)(pixel & 0x00FF0000) >> 16);
            return color;
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

        public void MouseMove(int dx, int dy)
        {
            if (!Focused)
                throw new Exception("genshin not in focus");
            Simulator.Mouse.MoveMouseBy(dx, dy);
        }

        

    }
}
