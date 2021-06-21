using genshinbot.automation.screenshot;
using genshinbot.reactive;
using genshinbot.reactive.wire;
using OpenCvSharp;
using System;

namespace genshinbot.automation
{
    public class ProxyScreen : ScreenshotObservable
    {
        ILiveWire<bool> enabled;
        ScreenshotObservable s;

        public ProxyScreen(ILiveWire<bool> enabled, ScreenshotObservable s)
        {
            this.enabled = enabled;
            this.s = s;
        }

        public IWire<Pkt<Mat>> Watch(Rect r)
        {
            return s.Watch(r).Relay(enabled);
        }
    }
}
