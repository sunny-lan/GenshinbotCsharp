using genshinbot.automation.screenshot;
using genshinbot.reactive;
using OpenCvSharp;
using System;

namespace genshinbot.automation
{
    public class ProxyScreen : ScreenshotObservable
    {
        IObservableValue<bool> enabled;
        ScreenshotObservable s;

        public ProxyScreen(IObservableValue<bool> enabled, ScreenshotObservable s)
        {
            this.enabled = enabled;
            this.s = s;
        }

        public IObservable<Pkt<Mat>> Watch(Rect r)
        {
            return s.Watch(r).Relay(enabled);
        }
    }
}
