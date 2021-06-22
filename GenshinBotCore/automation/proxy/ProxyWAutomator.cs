using genshinbot.automation.hooking;
using genshinbot.automation.input;
using genshinbot.automation.screenshot;
using genshinbot.reactive;
using genshinbot.reactive.wire;
using OpenCvSharp;
using System;
using System.Diagnostics;

namespace genshinbot.automation
{
    public class ProxyWAutomator : IWindowAutomator2
    {
        ILiveWire<bool> enabled;
        IWindowAutomator2 w;


        public ILiveWire<bool> Focused { get; private init; }

        public ILiveWire<Size?> Size { get; private init; }

        public IKeySimulator2 Keys { get; private init; }

        public IMouseSimulator2 Mouse { get; private init; }

        public ScreenshotObservable Screen { get; private init; }

        public IMouseCapture MouseCap => throw new NotImplementedException();

        public IKeyCapture KeyCap => throw new NotImplementedException();

        public ILiveWire<Rect?> ScreenBounds => throw new NotImplementedException();

        public ProxyWAutomator(ILiveWire<bool> enabled, IWindowAutomator2 w)
        {
            this.enabled = enabled;
            this.w = w;
            Keys = new ProxyKey(enabled, w.Keys);
            Mouse = new ProxyMouse(enabled, w.Mouse);
            Screen = new ProxyScreen(enabled, w.Screen);

            Focused = w.Focused;//TODO.Relay(enabled);
            Size = w.Size.Relay2(enabled);

        }
        public void TryFocus()
        {
            Debug.Assert(enabled.Value);
            w.TryFocus();
        }
    }
}
