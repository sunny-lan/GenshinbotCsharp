using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace genshinbot.automation.input.windows
{
    class InputSimulatorStandardAdapter : IMouseSimulator2, IKeySimulator2
    {
        public InputSimulatorStandard.InputSimulator iS = new InputSimulatorStandard.InputSimulator();
        public Task Key(Keys k, bool down)
        {
            if (down)
            {
                iS.Keyboard.KeyDown((InputSimulatorStandard.Native.VirtualKeyCode)k);
            }
            else
            {
                iS.Keyboard.KeyUp((InputSimulatorStandard.Native.VirtualKeyCode)k);
            }

            return Task.CompletedTask;
        }

        public Task MouseButton(MouseBtn btn, bool down)
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

            return Task.CompletedTask;
        }

        public Task MouseMove(Point2d d)
        {
            DPIAware.Use(DPIAware.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE, () =>
            {
                var oo = d.Round();
                iS.Mouse.MoveMouseBy(oo.X, oo.Y);
            });

            return Task.CompletedTask;
        }

        public Task<Point2d> MousePos()
        {
            return Task.FromResult(DPIAware.Use(DPIAware.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE, () =>
            {
                var pt = Cursor.Position;
                return pt.Cv().Cvt();
            }));
        }

        void cvtPixelToMouse(ref System.Drawing.Point p)
        {
            // User32.GetClientRect(User32.GetDesktopWindow(), out var desktop);
            p.X = 65536 * p.X / SystemInformation.VirtualScreen.Width;
            p.Y = 65536 * p.Y / SystemInformation.VirtualScreen.Height;
        }
        public Task MouseTo(Point2d p)
        {
            var pp = new System.Drawing.Point((int)Math.Round(p.X), (int)Math.Round(p.Y));
            DPIAware.Use(DPIAware.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE, () =>
            {
               
                cvtPixelToMouse(ref pp);

                /*SendMouseInput(new User32.MOUSEINPUT
                {
                    dx = pp.X,
                    dy = pp.Y,
                    dwFlags = (uint)User32.MOUSEEVENTF.MOUSEEVENTF_MOVE | (uint)User32.MOUSEEVENTF.MOUSEEVENTF_ABSOLUTE |(uint)User32.MOUSEEVENTF.MOUSEEVENTF_VIRTUALDESK
                });*/
            });
            iS.Mouse.MoveMouseToPositionOnVirtualDesktop(pp.X, pp.Y);
            return Task.CompletedTask;
        }
    }
}
