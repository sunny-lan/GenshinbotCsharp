using genshinbot.hooks;
using genshinbot.reactive;
using genshinbot.reactive.wire;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace genshinbot.automation.hooking
{
    public class MouseHookAdapter:IMouseCapture
    {

        public IWire<IMouseCapture.MouseEvent> MouseEvents { get;  }

        public MouseHook mouseHook;

        public MouseHookAdapter(ILiveWire<bool> enable, Func<Point,Point> map=null)
        {
            mouseHook = new MouseHook();

            MouseEvents = mouseHook.Wire
                .Relay(enable)
                .Link<(IntPtr wParam,User32.MOUSEHOOKSTRUCT lParam),IMouseCapture.MouseEvent>(( x, next) =>
                {
                    
                    var k = x.lParam;
                    var msg = (User32.WindowMessage)x.wParam;
                    var pt = k.pt.Cv();
                    if (map != null) pt = map(pt);
                    if (msg == User32.WindowMessage.WM_LBUTTONDOWN)
                    {
                         next(new IMouseCapture.ClickEvent
                        {
                            Button = automation.input.MouseBtn.Left,
                            Down = true,
                            Position = pt,
                        });
                    }
                    else if (msg == User32.WindowMessage.WM_MOUSEMOVE)
                    {
                         next(new IMouseCapture.MoveEvent
                        {
                            Position = pt
                        });
                    }
                    else if (msg == User32.WindowMessage.WM_LBUTTONUP)
                    {
                         next(new IMouseCapture.ClickEvent
                        {
                            Button = automation.input.MouseBtn.Left,
                            Down = true,
                            Position = pt
                        });
                    }

                });
            mouseHook.Start();
        }

        ~MouseHookAdapter()
        {
            mouseHook.Stop();
        }
    }
}
