using genshinbot.hooks;
using genshinbot.reactive;
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

        public IObservable<IMouseCapture.MouseEvent> MouseEvents { get; private init; }

        public MouseHook mouseHook;

        public MouseHookAdapter(IObservable<bool> enable, Func<Point,Point> map=null)
        {
            mouseHook = new MouseHook();

            MouseEvents = mouseHook
                .Relay(enable)
                .Select(x => {
                    var k = x.lParam;
                    var msg = (User32.WindowMessage)x.wParam;
                    var pt = k.pt.Cv();
                    if (map != null) pt = map(pt);
                    if (msg == User32.WindowMessage.WM_LBUTTONDOWN)
                    {
                        return new IMouseCapture.ClickEvent
                        {
                            Button = automation.input.MouseBtn.Left,
                            Down = true,
                            Position = pt,
                        };
                    }
                    else if (msg == User32.WindowMessage.WM_MOUSEMOVE)
                    {
                        return new IMouseCapture.MoveEvent
                        {
                            Position = pt
                        };
                    }
                    else if (msg == User32.WindowMessage.WM_LBUTTONUP)
                    {
                        return new IMouseCapture.ClickEvent
                        {
                            Button = automation.input.MouseBtn.Left,
                            Down = true,
                            Position =pt
                        };
                    }

                    return null as IMouseCapture.MouseEvent;
                }).NonNull().Publish().RefCount();
            mouseHook.Start();
        }

        ~MouseHookAdapter()
        {
            mouseHook.Stop();
        }
    }
}
