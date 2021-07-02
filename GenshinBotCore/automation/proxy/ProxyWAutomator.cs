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


        public ILiveWire<bool> Focused { get;  }

        public ILiveWire<Size?> Size { get;  }

        public IKeySimulator2 Keys { get;  }

        public IMouseSimulator2 Mouse { get;  }

        public ScreenshotObservable Screen { get;  }

        public IMouseCapture MouseCap { get; }

        public IKeyCapture KeyCap { get; }

        public ILiveWire<Rect?> ScreenBounds { get; }

        public ProxyWAutomator(ILiveWire<bool> enabled, IWindowAutomator2 w)
        {
            this.enabled = enabled;
            this.w = w;
            Keys = new ProxyKey(enabled, w.Keys);
            Mouse = new ProxyMouse(enabled, w.Mouse);
            Screen = new ProxyScreen(enabled, w.Screen);

            //TODO
            KeyCap = w.KeyCap;
            MouseCap = w.MouseCap;

            ScreenBounds = w.ScreenBounds.Relay2(enabled);

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
